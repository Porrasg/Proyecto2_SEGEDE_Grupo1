using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace CoreApp
{
    public class OtpManager
    {
        public string GenerateAndSaveOtp(string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                throw new Exception("El correo es requerido para generar el token OTP.");

            // RF-AUT-003: OTP de 6 dígitos numéricos estrictamente
            var random = new Random();
            string generatedToken = random.Next(100000, 999999).ToString();

            // RF-AUT-003: Vigencia por defecto de 3 minutos establecida por el sistema
            int expirationMinutes = 3;
            DateTime expirationTime = DateTime.Now.AddMinutes(expirationMinutes);

            // Instanciación del DTO e impacto inmutable en la persistencia local
            var otpDto = new OtpToken()
            {
                Email = userEmail,
                TokenCode = generatedToken,
                ExpirationDate = expirationTime,
                IsUsed = false
            };

            var otpCrud = new OtpCrudFactory();
            otpCrud.Create(otpDto);

            // Retornamos el token generado puramente en texto para que la capa superior pueda enviarlo por correo
            return generatedToken;
        }

        // NUEVO MÉTODO IMPLEMENTADO PARA EL REQUERIMIENTO DE ENVÍO (RF-AUT-004)
        public void SendOtpEmail(string toEmail, string userName, string token)
        {
            try
            {
                // Configuración de red estándar para servidores de salida de Google
                string smtpHost = "smtp.gmail.com";
                int smtpPort = 587;

                // CAMBIA ESTAS DOS LÍNEAS CON TUS DATOS PERSONALES QUE GENERASTE:
                string smtpUser = "hfloresq@ucenfotec.ac.cr";
                string smtpPassword = "fwmqhxxnmyzqalrz";

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(smtpUser, "SEGEDE - Sistema de Energía");
                    mail.To.Add(toEmail);
                    mail.Subject = "Activación de Cuenta - Código OTP de Seguridad";

                    // Diseño estructurado con HTML para una visualización limpia en la bandeja de entrada
                    mail.Body = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee; max-width: 600px;'>
                            <h3 style='color: #333;'>Estimado(a) {userName},</h3>
                            <p>Gracias por registrarse en nuestra plataforma de simulación energética.</p>
                            <p>Para completar la activación autónoma de su cuenta, ingrese el siguiente código de seguridad de un solo uso:</p>
                            <div style='background-color: #f8f9fa; padding: 15px; text-align: center; margin: 20px 0;'>
                                <h2 style='color: #007bff; letter-spacing: 5px; margin: 0; font-size: 32px;'>{token}</h2>
                            </div>
                            <p>Este código tiene una vigencia estricta de <strong>3 minutos</strong>.</p>
                            <hr style='border: none; border-top: 1px solid #eee;' />
                            <small style='color: #777;'>Si usted no solicitó esta cuenta, por favor ignore este mensaje.</small>
                        </div>";
                    mail.IsBodyHtml = true;

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
                // Encapsula y lanza cualquier falla de conexión o credenciales para que la API la intercepte
                throw new Exception($"Fallo crítico al despachar el correo SMTP: {ex.Message}");
            }
        }
    }

}

