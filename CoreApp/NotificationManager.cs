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
            if (HasEmptyFields(n))
            {
                throw new Exception("Todos los campos obligatorios deben estar completos");
            }

            if (n.UserId <= 0)
            {
                throw new Exception("Debe seleccionar un usuario válido");
            }

            var nCrud = new NotificationCrudFactory();

            nCrud.Create(n);
        }
        public void Update(Notification n)
        {
            var nCrud = new NotificationCrudFactory();

            nCrud.Update(n);
        }

        public void Delete(Notification n)
        {
            var nCrud = new NotificationCrudFactory();

            nCrud.Delete(n);
        }

        private bool HasEmptyFields(Notification notification)
        {
            return string.IsNullOrWhiteSpace(notification.Title) ||
                   string.IsNullOrWhiteSpace(notification.Message) ||
                   string.IsNullOrWhiteSpace(notification.NotificationType);
        }
    }
}