using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
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
    }
}