using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DataAccess.CRUD;
using Entities_DTOs;

namespace CoreApp
{
    public class OtpManager
    {
        // Genera y guarda un OTP en la base de datos
        public string GenerateAndSaveOtp(string userEmail, string purpose)
        {
            userEmail = userEmail?.Trim(); 
            purpose = purpose?.Trim();

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new Exception("El correo es requerido para generar el token OTP.");
            }

            if (!IsValidPurpose(purpose))
            {
                throw new Exception("El propósito del código OTP no es válido.");
            }

            // RF-AUT-003: OTP de 6 dígitos numéricos estrictamente
            string generatedToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            // RF-AUT-003: Vigencia por defecto de 3 minutos establecida por el sistema
            int expirationMinutes = 3;
            DateTime expirationTime = DateTime.Now.AddMinutes(expirationMinutes);

            // Instanciación del DTO
            var otpDto = new OtpToken()
            {
                Email = userEmail,
                TokenCode = generatedToken,
                Purpose = purpose,
                ExpirationDate = expirationTime,
                IsUsed = false,
                CreatedAt = DateTime.Now
            };

            var otpCrud = new OtpCrudFactory();

            // Invalida códigos anteriores para el mismo correo y propósito
            otpCrud.InvalidatePreviousOtps(userEmail, purpose);

            // Guarda el nuevo código
            otpCrud.Create(otpDto);

            // Retornamos el token generado
            return generatedToken;
        }

        // Genera, guarda y envía el OTP
        public void GenerateAndSendOtp(
            string userEmail,
            string userName,
            string purpose)
        {
            string generatedToken =
                GenerateAndSaveOtp(
                    userEmail,
                    purpose
                );

            SendOtpEmail(
                userEmail,
                userName,
                generatedToken,
                purpose
            );
        }

        // Valida el OTP ingresado por el usuario
        public void ValidateOtp(string email,string tokenCode,string purpose)
        {
            email = email?.Trim();
            tokenCode = tokenCode?.Trim();
            purpose = purpose?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El correo electrónico es requerido.");
            }

            if (string.IsNullOrWhiteSpace(tokenCode))
            {
                throw new Exception("El código OTP es requerido.");
            }

            if (!IsValidOtpFormat(tokenCode))
            {
                throw new Exception("El código OTP debe contener exactamente 6 dígitos.");
            }

            if (!IsValidPurpose(purpose))
            {
                throw new Exception("El propósito del código OTP no es válido.");
            }

            var otpCrud = new OtpCrudFactory();

            var otp = otpCrud.RetrieveValidOtp(email, tokenCode, purpose);

            if (otp == null)
            {
                throw new Exception("El código OTP es incorrecto o ha expirado.");
            }

            if (otp.ExpirationDate < DateTime.Now)
            {
                throw new Exception("El código OTP ha expirado.");
            }

            // El OTP puede validarse múltiples veces hasta expirar.
        }

        // Envía el OTP por correo electrónico (RF-AUT-004)
        public void SendOtpEmail(string toEmail, string userName, string token, string purpose)
        {
            try
            {
                // Configuración de red estándar para servidores de salida de Google
                string smtpHost = "smtp.gmail.com";
                int smtpPort = 587;

                // Se obtienen las credenciales desde variables de entorno
                string smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
                string smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

                // Validación de credenciales SMTP
                if (string.IsNullOrWhiteSpace(smtpUser) ||
                    string.IsNullOrWhiteSpace(smtpPassword))
                {
                    return;
                }

                // Se obtiene el asunto del correo según el propósito
                string emailSubject = GetEmailSubject(purpose);

                // Se obtiene el mensaje del correo según el propósito
                string purposeMessage = GetPurposeMessage(purpose);

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(smtpUser, "SEGEDE - Sistema de Energía");
                    mail.To.Add(toEmail);
                    mail.Subject = emailSubject;

                    // Diseño estructurado con HTML para una visualización limpia en la bandeja de entrada
                    mail.Body = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee;max-width: 600px;'>
                            <h3 style='color: #333;'>Estimado(a) {userName}, </h3>
                            <p>{purposeMessage}</p>
                            <p>Ingrese el siguiente código de seguridad de un solo uso: </p>
                            <div style='background-color: #f8f9fa; padding: 15px; text-align: center; margin: 20px 0;'>
                                <h2 style='color: #007bff; letter-spacing: 5px; margin: 0; font-size: 32px;'>{token}</h2>
                            </div>
                            <p>Este código tiene una vigencia de <strong>3 minutos</strong>.</p>
                            <hr style='border: none; border-top: 1px solid #eee;' />
                            <small style='color: #777;'>Si usted no solicitó esta operación, ignore este mensaje.</small>
                        </div>";
                    mail.IsBodyHtml = true;

                    // Configuración del cliente SMTP y envío del correo
                    using (var smtp = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(smtpUser, smtpPassword);
                        smtp.EnableSsl = true; // Exigido de forma estricta por los servidores de Google
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                // Si el correo no se puede enviar, se mantiene el OTP guardado y se informa la falla.
                throw new Exception($"No se pudo enviar el correo OTP: {ex.Message}");
            }
        }

        //Validaciones
        // Verifica que el OTP tenga exactamente 6 números
        private bool IsValidOtpFormat(string tokenCode)
        {
            if (tokenCode.Length != 6)
            {
                return false;
            }

            return Regex.IsMatch(tokenCode,
                    @"^\d{6}$");
        }

        // Verifica que el propósito del OTP sea válido
        private bool IsValidPurpose(string purpose)
        {
            return purpose == "ACCOUNT_ACTIVATION" ||
                   purpose == "LOGIN" ||
                   purpose == "CHANGE_PASSWORD" ||
                   purpose == "RESET_PASSWORD";
        }

        // Define el asunto del correo según la operación
        private string GetEmailSubject(string purpose)
        {
            if (purpose == "ACCOUNT_ACTIVATION")
            {
                return "Activación de cuenta - Código OTP";
            }

            if (purpose == "LOGIN")
            {
                return "Inicio de sesión - Código OTP";
            }

            if (purpose == "CHANGE_PASSWORD")
            {
                return "Cambio de contraseña - Código OTP";
            }

            if (purpose == "RESET_PASSWORD")
            {
                return "Restablecimiento de contraseña - Código OTP";
            }

            return "Código OTP de seguridad";
        }

        // Define el mensaje según la operación
        private string GetPurposeMessage(string purpose)
        {
            if (purpose == "ACCOUNT_ACTIVATION")
            {
                return "Para completar la activación de su cuenta, " +
                       "utilice el siguiente código.";
            }

            if (purpose == "LOGIN")
            {
                return "Para completar el inicio de sesión, " +
                       "utilice el siguiente código.";
            }

            if (purpose == "CHANGE_PASSWORD")
            {
                return "Para autorizar el cambio de contraseña, " +
                       "utilice el siguiente código.";
            }

            if (purpose == "RESET_PASSWORD")
            {
                return "Para restablecer su contraseña, " +
                       "utilice el siguiente código.";
            }

            return "Utilice el siguiente código para completar " +
                   "la operación solicitada.";
        }

    }

}

