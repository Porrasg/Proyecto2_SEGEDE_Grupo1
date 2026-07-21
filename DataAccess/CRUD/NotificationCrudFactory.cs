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
            var note = baseDTO as Notification;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_NOTIFICATION_PR";

            sqlOperation.AddIntParameter("UserId", note.UserId);
            sqlOperation.AddStringParameter("Title", note.Title);
            sqlOperation.AddStringParameter("Message", note.Message);
            sqlOperation.AddStringParameter("NotificationType", note.NotificationType);
            sqlOperation.AddStringParameter("ReferenceType", note.ReferenceType);
            sqlOperation.AddIntParameter("ReferenceId", note.ReferenceId ?? 0);
            sqlOperation.AddIntParameter("IsRead", note.IsRead ? 1 : 0);
            sqlOperation.AddDateTimeParameter("ReadAt", note.ReadAt ?? default(DateTime));
            sqlOperation.AddDateTimeParameter("CreatedAt", note.CreatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var note = baseDTO as Notification;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_NOTIFICATION_PR";

            sqlOperation.AddIntParameter("NotificationId", note.Id);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();
            var op = new SqlOperation();
            op.ProcedureName = "RET_ALL_NOTIFICATION_PR";
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var n = BuildNotification(row);
                    list.Add((T)Convert.ChangeType(n, typeof(T)));
                }
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var op = new SqlOperation();
            op.ProcedureName = "RET_BY_ID_NOTIFICATION_PR";
            op.AddIntParameter("NotificationId", id);
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                var n = BuildNotification(results[0]);
                return (T)Convert.ChangeType(n, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var note = baseDTO as Notification;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_NOTIFICATION_PR";

            sqlOperation.AddIntParameter("NotificationId", note.Id);
            sqlOperation.AddIntParameter("UserId", note.UserId);
            sqlOperation.AddStringParameter("Title", note.Title);
            sqlOperation.AddStringParameter("Message", note.Message);
            sqlOperation.AddStringParameter("NotificationType", note.NotificationType);
            sqlOperation.AddStringParameter("ReferenceType", note.ReferenceType);
            sqlOperation.AddIntParameter("ReferenceId", note.ReferenceId ?? 0);
            sqlOperation.AddIntParameter("IsRead", note.IsRead ? 1 : 0);
            sqlOperation.AddDateTimeParameter("ReadAt", note.ReadAt ?? default(DateTime));

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        private Notification BuildNotification(Dictionary<string, object> row)
        {
            var n = new Notification
            {
                Id = (int)row["NotificationId"],
                UserId = (int)row["UserId"],
                Title = (string)row["Title"],
                Message = (string)row["Message"],
                NotificationType = (string)row["NotificationType"],
                ReferenceType = row.ContainsKey("ReferenceType") && row["ReferenceType"] != DBNull.Value ? row["ReferenceType"].ToString() : null,
                ReferenceId = row.ContainsKey("ReferenceId") && row["ReferenceId"] != DBNull.Value ? (int?)Convert.ToInt32(row["ReferenceId"]) : null,
                IsRead = row.ContainsKey("IsRead") && row["IsRead"] != DBNull.Value ? Convert.ToBoolean(row["IsRead"]) : false,
                ReadAt = row.ContainsKey("ReadAt") && row["ReadAt"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["ReadAt"]) : null,
                CreatedAt = row.ContainsKey("CreatedAt") && row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]) : default(DateTime)
            };
            return n;
        }
    }
}
