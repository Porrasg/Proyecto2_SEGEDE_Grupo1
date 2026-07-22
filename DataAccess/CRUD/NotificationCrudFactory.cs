using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class NotificationCrudFactory : CrudFactory
    {

        public NotificationCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }
        public override void Create(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Notification
            var notification = baseDTO as Notification;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_NOTIFICATION_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddIntParameter("UserId", notification.UserId);
            sqlOperation.AddStringParameter("Title", notification.Title);
            sqlOperation.AddStringParameter("Message", notification.Message);
            sqlOperation.AddStringParameter("NotificationType", notification.NotificationType);
            sqlOperation.AddNullableStringParameter("ReferenceType", notification.ReferenceType); // puede ser nulo si la notificacion no referencia una entidad
            sqlOperation.AddNullableIntParameter("ReferenceId", notification.ReferenceId); // puede ser nulo si la notificacion no referencia un registro
            sqlOperation.AddIntParameter("IsRead", notification.IsRead ? 1 : 0);
            sqlOperation.AddNullableDateTimeParameter("ReadAt", notification.ReadAt); // puede ser nulo si aun no se ha leido
            sqlOperation.AddDateTimeParameter("CreatedAt", notification.CreatedAt);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Notification
            var notification = baseDTO as Notification;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_NOTIFICATION_PR";

            sqlOperation.AddIntParameter("NotificationId", notification.Id);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todas las notificaciones que se obtengan de la consulta a la BD
            var lsNotifications = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_NOTIFICATION_PR";

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Notification, luego agregarlo a la lista de notificaciones
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var notification = BuildNotification(row);
                    lsNotifications.Add((T)Convert.ChangeType(notification, typeof(T)));
                }
            }

            return lsNotifications;
        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_NOTIFICATION_PR";

            sqlOperation.AddIntParameter("NotificationId", id);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Notification y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var notification = BuildNotification(lsResults[0]);
                return (T)Convert.ChangeType(notification, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Notification
            var notification = baseDTO as Notification;

            // Definir el SP
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "UPD_NOTIFICATION_PR";

            sqlOperation.AddIntParameter("NotificationId", notification.Id);
            sqlOperation.AddIntParameter("UserId", notification.UserId);
            sqlOperation.AddStringParameter("Title", notification.Title);
            sqlOperation.AddStringParameter("Message", notification.Message);
            sqlOperation.AddStringParameter("NotificationType", notification.NotificationType);
            sqlOperation.AddNullableStringParameter("ReferenceType", notification.ReferenceType); // puede ser nulo si la notificacion no referencia una entidad
            sqlOperation.AddNullableIntParameter("ReferenceId", notification.ReferenceId); // puede ser nulo si la notificacion no referencia un registro
            sqlOperation.AddIntParameter("IsRead", notification.IsRead ? 1 : 0);
            sqlOperation.AddNullableDateTimeParameter("ReadAt", notification.ReadAt); // puede ser nulo si aun no se ha leido

            //Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        //Metodo que construye el DTO del Notification a partir de la data que viene en la consulta de la BD
        private Notification BuildNotification(Dictionary<string, object> row)
        {
            var notification = new Notification
            {
                Id = (int)row["NotificationId"],
                UserId = (int)row["UserId"],
                Title = (string)row["Title"],
                Message = (string)row["Message"],
                NotificationType = (string)row["NotificationType"],
                ReferenceType = row["ReferenceType"] != DBNull.Value ? (string)row["ReferenceType"] : null, // para manejar el caso de que ReferenceType pueda ser nulo
                ReferenceId = row["ReferenceId"] != DBNull.Value ? (int?)Convert.ToInt32(row["ReferenceId"]) : null, // para manejar el caso de que ReferenceId pueda ser nulo
                IsRead = Convert.ToBoolean(row["IsRead"]),
                ReadAt = row["ReadAt"] != DBNull.Value ? (DateTime?)row["ReadAt"] : null, // para manejar el caso de que ReadAt pueda ser nulo
                CreatedAt = (DateTime)row["CreatedAt"]
            };
            return notification;
        }
    }
}
