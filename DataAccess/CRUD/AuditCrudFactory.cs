using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class AuditCrudFactory : CrudFactory
    {

        public AuditCrudFactory() {
            // Este pide el acceso a la base de datos 
            sqlDao = SqlDao.GetInstance();  // sirve para usar simpre la misma conexion de la base de datos y no abrir una nueva cadena  

        }

        //Este método es para CREAR una nueva notificación en la base de datos.
        public override void Create(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Audit
            var audit = baseDTO as Audit;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "CRE_AUDIT_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddIntParameter("UserId", audit.UserId ?? 0);
            sqlOperation.AddStringParameter("Action", audit.Action);
            sqlOperation.AddStringParameter("EntityName", audit.EntityName);
            sqlOperation.AddIntParameter("EntityId", audit.EntityId ?? 0);
            sqlOperation.AddStringParameter("Description", audit.Description);
            sqlOperation.AddStringParameter("IpAddress", audit.IpAddress);
            sqlOperation.AddDateTimeParameter("CreatedAt", audit.CreatedAt);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);

        }




        // Este método es para ELIMINAR una notificación de la base de datos.
        public override void Delete(BaseDTO baseDTO)
        {
            // 1. Convertir el baseDTO en un objeto Audit
            var audit = baseDTO as Audit;

            // 2. Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "DEL_AUDIT_PR";

            // 3. Pasar el parámetro exacto como está en el SP SQL ("AuditId")
            // Usamos AddIntParameter porque AuditId es de tipo INT en la base de datos
            sqlOperation.AddIntParameter("AuditId", audit.Id);

            // 4. Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }




        // Este método es para TRAER/CONSULTAR la lista con todas las notificaciones.
        public override List<T> RetrieveAll<T>()
        {
            var lsUsers = new List<T>();

            var Operation = new SqlOperation();
            Operation.ProcedureName = "RET_ALL_AUDIT_PR";


            var lsResults = sqlDao.ExecuteQueryProcedure(Operation);

            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var audit = BuildAudit(row);
                    lsUsers.Add((T)Convert.ChangeType(audit, typeof(T)));
                }
            }

            return lsUsers;

        }



        // Este método es para BUSCAR una notificación específica usando su ID.
        public override T RetrieveById<T>(int id)
        {
            var operation = new SqlOperation();
            operation.ProcedureName = "RET_BY_ID_AUDIT_PR";
            operation.AddIntParameter("AuditId", id);
            var lsResults = sqlDao.ExecuteQueryProcedure(operation);
            if (lsResults.Count > 0)
            {
                var item = lsResults[0];
                var audit = BuildAudit(lsResults[0]);
                return (T)Convert.ChangeType(audit, typeof(T));
            }
            return default(T);
        }

        // Este método es para ACTUALIZAR/MODIFICAR una notificación existente.
        public override void Update(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Audit
            var audit = baseDTO as Audit;

            // definir el SP por medio del sql operation

            var sqlOperation = new SqlOperation();


            sqlOperation.ProcedureName = "UPD_AUDIT_PR";


            sqlOperation.AddIntParameter("AuditId", audit.Id);
            sqlOperation.AddIntParameter("UserId", audit.UserId ?? 0);
            sqlOperation.AddStringParameter("Action", audit.Action);
            sqlOperation.AddStringParameter("EntityName", audit.EntityName);
            sqlOperation.AddIntParameter("EntityId", audit.EntityId ?? 0);
            sqlOperation.AddStringParameter("Description", audit.Description);
            sqlOperation.AddStringParameter("IpAddress", audit.IpAddress);



            //Ejecutamso el SP
            sqlDao.ExecuteProcedure(sqlOperation);

        }

        private Audit BuildAudit(Dictionary<string, object> row)
        {
            var audit = new Audit
            {
                Id = (int)row["AuditId"],
                UserId = (int)row["UserId"],
                Action = (string)row["Action"],
                EntityName = (string)row["EntityName"],
                EntityId = (int)row["EntityId"],
                Description = (string)row["Description"],
                IpAddress = (string)row["IpAddress"],
                CreatedAt = (DateTime)row["CreatedAt"]
            };
            return audit;
        }
    }
}
