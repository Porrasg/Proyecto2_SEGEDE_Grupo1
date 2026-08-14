using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace CoreApp
{
    public class NotificationManager
    {
        public List<Notification> RetrieveAllNotifications()
        {
            var nCrud = new NotificationCrudFactory();
            return nCrud.RetrieveAll<Notification>();
        }

        public void Create(Notification n)
        {
            // Validar que la notificación no sea nula
            if (n == null)
            {
                throw new Exception("La notificación no puede ser nula");
            }

            // Validar que los campos obligatorios estén completos
            if (HasEmptyFields(n))
            {
                throw new Exception("Todos los campos obligatorios deben estar completos");
            }

            // Validar que el usuario sea válido
            if (n.UserId <= 0)
            {
                throw new Exception("Debe seleccionar un usuario válido");
            }

            // Validar la referencia de la notificación
            if (HasInvalidReference(n))
            {
                throw new Exception("El tipo y el identificador de referencia deben completarse juntos");
            }

            var userCrud = new UserCrudFactory();

            // Obtener el usuario relacionado
            var user = userCrud.RetrieveById<User>(n.UserId);

            // Validar que el usuario exista
            if (user == null)
            {
                throw new Exception("El usuario seleccionado no existe");
            }

            // Asignar el estado inicial de la notificación
            n.IsRead = false;

            // La notificación todavía no ha sido leída
            n.ReadAt = null;

            // Asignar la fecha de creación
            n.CreatedAt = DateTime.Now;

            var nCrud = new NotificationCrudFactory();

            // Crear la notificación
            nCrud.Create(n);
        }


        public void Update(Notification n)
        {
            // Validar que la notificación no sea nula
            if (n == null)
            {
                throw new Exception("La notificación no puede ser nula");
            }

            // Validar el identificador de la notificación
            if (n.Id <= 0)
            {
                throw new Exception("El identificador de la notificación no es válido");
            }

            var nCrud = new NotificationCrudFactory();

            // Obtener la notificación registrada
            var currentNotification =
                nCrud.RetrieveById<Notification>(n.Id);

            // Validar que la notificación exista
            if (currentNotification == null)
            {
                throw new Exception("La notificación que desea actualizar no existe");
            }

            // Mantener la información original de la notificación
            n.UserId = currentNotification.UserId;
            n.Title = currentNotification.Title;
            n.Message = currentNotification.Message;
            n.NotificationType = currentNotification.NotificationType;
            n.ReferenceType = currentNotification.ReferenceType;
            n.ReferenceId = currentNotification.ReferenceId;
            n.CreatedAt = currentNotification.CreatedAt;

            // Asignar la fecha en que se leyó la notificación
            if (n.IsRead)
            {
                // Mantener la fecha anterior si ya estaba marcada como leída
                n.ReadAt = currentNotification.ReadAt ?? DateTime.Now;
            }
            else
            {
                // Eliminar la fecha si se marca nuevamente como no leída
                n.ReadAt = null;
            }

            // Actualizar la notificación
            nCrud.Update(n);
        }


        public void Delete(Notification n)
        {
            // Validar que la notificación no sea nula
            if (n == null)
            {
                throw new Exception("La notificación no puede ser nula");
            }

            // Validar el identificador de la notificación
            if (n.Id <= 0)
            {
                throw new Exception(
                    "El identificador de la notificación no es válido");
            }

            var nCrud = new NotificationCrudFactory();

            // Obtener la notificación registrada
            var currentNotification = nCrud.RetrieveById<Notification>(n.Id);

            // Validar que la notificación exista
            if (currentNotification == null)
            {
                throw new Exception(
                    "La notificación que desea eliminar no existe");
            }

            // Eliminar la notificación
            nCrud.Delete(n);
        }


        private bool HasEmptyFields(Notification notification)
        {
            return string.IsNullOrWhiteSpace(notification.Title) ||
                   string.IsNullOrWhiteSpace(notification.Message) ||
                   string.IsNullOrWhiteSpace(notification.NotificationType);
        }

        private bool HasInvalidReference(Notification notification)
        {
            bool hasReferenceType =
                !string.IsNullOrWhiteSpace(notification.ReferenceType);

            bool hasReferenceId =
                notification.ReferenceId.HasValue &&
                notification.ReferenceId.Value > 0;

            return hasReferenceType != hasReferenceId;
        }


        // ========================================================================
        // ENVÍO DE NOTIFICACIONES POR CORREO ELECTRÓNICO
        // ========================================================================

        // Envía una notificación genérica por correo electrónico
        // Reutiliza la misma infraestructura SMTP que OtpManager
        public void SendNotification(string toEmail, string userName, string subject, string bodyHtml)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            string smtpHost = "smtp.gmail.com";
            int smtpPort = 587;

            string smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            string smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

            // Si no hay credenciales SMTP configuradas, no se envía el correo
            // pero no se bloquea la operación de negocio
            if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPassword))
            {
                return;
            }

            try
            {
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(smtpUser, "SEGEDE - Sistema de Energía");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee; max-width: 600px;'>
                            <h3 style='color: #333;'>Estimado(a) {userName},</h3>
                            <p>{bodyHtml}</p>
                            <hr style='border: none; border-top: 1px solid #eee;' />
                            <small style='color: #777;'>Este es un correo automático del sistema SEGEDE, no responda a este mensaje.</small>
                        </div>";
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(smtpUser, smtpPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"No se pudo enviar la notificación por correo: {ex.Message}");
            }
        }


        // ========================================================================
        // MÉTODOS ESPECÍFICOS DE NOTIFICACIÓN
        // ========================================================================

        // Envía notificación de agendamiento de servicio técnico en turbinas
        public void SendServiceScheduledNotification(
            string toEmail,
            string userName,
            string turbineName,
            string serviceDate)
        {
            if (!IsValidNotificationPurpose("SERVICE_SCHEDULED"))
            {
                throw new Exception("El propósito de notificación no es válido.");
            }

            string subject = GetNotificationSubject("SERVICE_SCHEDULED");
            string bodyHtml = GetNotificationMessage(
                "SERVICE_SCHEDULED",
                userName,
                $"Se ha agendado un servicio técnico para la turbina <strong>{turbineName}</strong> " +
                $"con fecha programada: <strong>{serviceDate}</strong>.<br/><br/>" +
                $"Por favor, tome las precauciones necesarias y coordine con el personal técnico."
            );

            SendNotification(toEmail, userName, subject, bodyHtml);
        }


        // Envía notificación de activación de alarma de criticidad sistémica
        public void SendCriticalAlarmNotification(
            string toEmail,
            string userName,
            string alarmDescription,
            string severity)
        {
            if (!IsValidNotificationPurpose("CRITICAL_ALARM"))
            {
                throw new Exception("El propósito de notificación no es válido.");
            }

            string subject = GetNotificationSubject("CRITICAL_ALARM");
            string bodyHtml = GetNotificationMessage(
                "CRITICAL_ALARM",
                userName,
                $"Se ha activado una alarma de criticidad <strong style='color: #dc3545;'>{severity}</strong>:<br/><br/>" +
                $"<strong>Descripción:</strong> {alarmDescription}<br/><br/>" +
                $"Se requiere atención inmediata para evaluar y resolver la situación."
            );

            SendNotification(toEmail, userName, subject, bodyHtml);
        }


        // Envía notificación de asignación de cuota de energía
        public void SendEnergyQuotaNotification(
            string toEmail,
            string userName,
            string quotaAmount,
            string period)
        {
            if (!IsValidNotificationPurpose("ENERGY_QUOTA_ASSIGNED"))
            {
                throw new Exception("El propósito de notificación no es válido.");
            }

            string subject = GetNotificationSubject("ENERGY_QUOTA_ASSIGNED");
            string bodyHtml = GetNotificationMessage(
                "ENERGY_QUOTA_ASSIGNED",
                userName,
                $"Se le ha asignado una cuota de energía de <strong>{quotaAmount}</strong> " +
                $"para el período: <strong>{period}</strong>.<br/><br/>" +
                $"Puede consultar los detalles en el sistema."
            );

            SendNotification(toEmail, userName, subject, bodyHtml);
        }


        // Envía notificación de bloqueo de cuenta por intentos fallidos
        public void SendAccountLockedNotification(
            string toEmail,
            string userName,
            int remainingMinutes)
        {
            if (!IsValidNotificationPurpose("ACCOUNT_LOCKED"))
            {
                throw new Exception("El propósito de notificación no es válido.");
            }

            string subject = GetNotificationSubject("ACCOUNT_LOCKED");
            string bodyHtml = GetNotificationMessage(
                "ACCOUNT_LOCKED",
                userName,
                $"Su cuenta ha sido <strong style='color: #dc3545;'>bloqueada temporalmente</strong> " +
                $"debido a múltiples intentos fallidos de inicio de sesión.<br/><br/>" +
                $"<strong>Tiempo de bloqueo:</strong> {remainingMinutes} minutos.<br/><br/>" +
                $"Si no reconoce esta actividad, por favor contacte al administrador del sistema."
            );

            SendNotification(toEmail, userName, subject, bodyHtml);
        }


        // Envía notificación de inicio de sesión exitoso después de validar OTP
        public void SendLoginSuccessNotification(
            string toEmail,
            string userName,
            string loginDateTime)
        {
            if (!IsValidNotificationPurpose("LOGIN_SUCCESSFUL"))
            {
                throw new Exception("El propósito de notificación no es válido.");
            }

            string subject = GetNotificationSubject("LOGIN_SUCCESSFUL");
            string bodyHtml = GetNotificationMessage(
                "LOGIN_SUCCESSFUL",
                userName,
                $"Ha iniciado sesión correctamente en el sistema SEGEDE.<br/><br/>" +
                $"<strong>Fecha y hora:</strong> {loginDateTime}<br/><br/>" +
                $"Si no reconoce esta actividad, por favor cambie su contraseña inmediatamente " +
                $"y contacte al administrador del sistema."
            );

            SendNotification(toEmail, userName, subject, bodyHtml);
        }


        // Envía notificación de factura generada a un comprador
        public void SendInvoiceGeneratedNotification(
            string toEmail,
            string userName,
            string invoiceNumber,
            string totalAmount,
            string dueDate)
        {
            if (!IsValidNotificationPurpose("INVOICE_GENERATED"))
            {
                throw new Exception("El propósito de notificación no es válido.");
            }

            string subject = GetNotificationSubject("INVOICE_GENERATED");
            string bodyHtml = GetNotificationMessage(
                "INVOICE_GENERATED",
                userName,
                $"Se ha generado una nueva factura a su nombre:<br/><br/>" +
                $"<strong>Número de factura:</strong> {invoiceNumber}<br/>" +
                $"<strong>Monto total:</strong> {totalAmount}<br/>" +
                $"<strong>Fecha de vencimiento:</strong> <span style='color: #dc3545;'>{dueDate}</span><br/><br/>" +
                $"Puede consultar los detalles de su factura en el sistema."
            );

            SendNotification(toEmail, userName, subject, bodyHtml);
        }


        // Envía notificación de factura vencida
        public void SendInvoiceOverdueNotification(
            string toEmail,
            string userName,
            string invoiceNumber,
            string totalAmount,
            int daysOverdue)
        {
            if (!IsValidNotificationPurpose("INVOICE_OVERDUE"))
            {
                throw new Exception("El propósito de notificación no es válido.");
            }

            string subject = GetNotificationSubject("INVOICE_OVERDUE");
            string bodyHtml = GetNotificationMessage(
                "INVOICE_OVERDUE",
                userName,
                $"<strong style='color: #dc3545;'>AVISO IMPORTANTE:</strong> Su factura ha vencido.<br/><br/>" +
                $"<strong>Número de factura:</strong> {invoiceNumber}<br/>" +
                $"<strong>Monto total:</strong> {totalAmount}<br/>" +
                $"<strong>Días de atraso:</strong> <span style='color: #dc3545;'>{daysOverdue}</span><br/><br/>" +
                $"Por favor, realice el pago lo antes posible para evitar interrupciones en el servicio."
            );

            SendNotification(toEmail, userName, subject, bodyHtml);
        }


        // Envía notificación de falla reportada en turbina
        public void SendTurbineFailureNotification(
            string toEmail,
            string userName,
            string turbineName,
            string severity,
            string description)
        {
            if (!IsValidNotificationPurpose("TURBINE_FAILURE_REPORTED"))
            {
                throw new Exception("El propósito de notificación no es válido.");
            }

            string subject = GetNotificationSubject("TURBINE_FAILURE_REPORTED");
            string bodyHtml = GetNotificationMessage(
                "TURBINE_FAILURE_REPORTED",
                userName,
                $"Se ha reportado una falla en la turbina <strong>{turbineName}</strong>:<br/><br/>" +
                $"<strong>Severidad:</strong> <span style='color: #dc3545;'>{severity}</span><br/>" +
                $"<strong>Descripción:</strong> {description}<br/><br/>" +
                $"Se requiere atención de ingeniería para evaluar y resolver la situación."
            );

            SendNotification(toEmail, userName, subject, bodyHtml);
        }


        // ========================================================================
        // VALIDACIONES Y MAPEOS
        // ========================================================================

        // Verifica que el propósito de la notificación sea válido
        private bool IsValidNotificationPurpose(string purpose)
        {
            return purpose == "SERVICE_SCHEDULED" ||
                   purpose == "CRITICAL_ALARM" ||
                   purpose == "ENERGY_QUOTA_ASSIGNED" ||
                   purpose == "ACCOUNT_LOCKED" ||
                   purpose == "LOGIN_SUCCESSFUL" ||
                   purpose == "INVOICE_GENERATED" ||
                   purpose == "INVOICE_OVERDUE" ||
                   purpose == "TURBINE_FAILURE_REPORTED";
        }


        // Define el asunto del correo según el propósito de la notificación
        private string GetNotificationSubject(string purpose)
        {
            if (purpose == "SERVICE_SCHEDULED")
            {
                return "Agendamiento de Servicio Técnico - SEGEDE";
            }

            if (purpose == "CRITICAL_ALARM")
            {
                return "⚠️ Alerta Crítica del Sistema - SEGEDE";
            }

            if (purpose == "ENERGY_QUOTA_ASSIGNED")
            {
                return "Asignación de Cuota de Energía - SEGEDE";
            }

            if (purpose == "ACCOUNT_LOCKED")
            {
                return "🔒 Cuenta Bloqueada Temporalmente - SEGEDE";
            }

            if (purpose == "LOGIN_SUCCESSFUL")
            {
                return "✅ Inicio de Sesión Exitoso - SEGEDE";
            }

            if (purpose == "INVOICE_GENERATED")
            {
                return "📄 Nueva Factura Generada - SEGEDE";
            }

            if (purpose == "INVOICE_OVERDUE")
            {
                return "⚠️ Factura Vencida - SEGEDE";
            }

            if (purpose == "TURBINE_FAILURE_REPORTED")
            {
                return "🔧 Falla Reportada en Turbina - SEGEDE";
            }

            return "Notificación del Sistema - SEGEDE";
        }


        // Define el mensaje del correo según el propósito de la notificación
        private string GetNotificationMessage(string purpose, string userName, string details)
        {
            string intro = "";

            if (purpose == "SERVICE_SCHEDULED")
            {
                intro = "Le informamos que:";
            }
            else if (purpose == "CRITICAL_ALARM")
            {
                intro = "ATENCIÓN - ALERTA DEL SISTEMA:";
            }
            else if (purpose == "ENERGY_QUOTA_ASSIGNED")
            {
                intro = "Le notificamos lo siguiente:";
            }
            else if (purpose == "ACCOUNT_LOCKED")
            {
                intro = "ATENCIÓN - SEGURIDAD DE CUENTA:";
            }
            else if (purpose == "LOGIN_SUCCESSFUL")
            {
                intro = "Notificación de seguridad:";
            }
            else if (purpose == "INVOICE_GENERATED")
            {
                intro = "Le informamos que:";
            }
            else if (purpose == "INVOICE_OVERDUE")
            {
                intro = "ATENCIÓN - PAGO PENDIENTE:";
            }
            else if (purpose == "TURBINE_FAILURE_REPORTED")
            {
                intro = "ALERTA TÉCNICA:";
            }
            else
            {
                intro = "Información del sistema:";
            }

            return $"{intro}<br/><br/>{details}";
        }
    }
}