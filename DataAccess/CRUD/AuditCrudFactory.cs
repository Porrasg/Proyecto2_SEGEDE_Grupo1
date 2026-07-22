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
            sqlDao = SqlDao.GetInstance();

        }

        public override void Create(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Audit
            var audit = baseDTO as Audit;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_AUDIT_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddNullableIntParameter("UserId", audit.UserId);
            sqlOperation.AddStringParameter("Action", audit.Action);
            sqlOperation.AddStringParameter("EntityName", audit.EntityName);
            sqlOperation.AddNullableIntParameter("EntityId", audit.EntityId);
            sqlOperation.AddStringParameter("Description", audit.Description);
            sqlOperation.AddNullableStringParameter("IpAddress", audit.IpAddress);
            sqlOperation.AddDateTimeParameter("CreatedAt", audit.CreatedAt);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);

        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Audit
            var audit = baseDTO as Audit;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_AUDIT_PR";

            sqlOperation.AddIntParameter("AuditId", audit.Id);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todas las auditorias que se obtengan de la consulta a la BD
            var lsAudits = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_AUDIT_PR";

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Audit, luego agregarlo a la lista de auditorias
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var audit = BuildAudit(row);
                    lsAudits.Add((T)Convert.ChangeType(audit, typeof(T)));
                }
            }

            return lsAudits;

        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_AUDIT_PR";

            sqlOperation.AddIntParameter("AuditId", id);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Audit y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var audit = BuildAudit(lsResults[0]);
                return (T)Convert.ChangeType(audit, typeof(T)); 
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Audit
            var audit = baseDTO as Audit;

            // Definir el SP
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

        //Metodo que construye el DTO del Audit a partir de la data que viene en la consulta de la BD
        private Audit BuildAudit(Dictionary<string, object> row)
        {
            var audit = new Audit
            {
                Id = (int)row["AuditId"],
                UserId = row["UserId"] != DBNull.Value ? (int?)Convert.ToInt32(row["UserId"]) : null, // para manejar el caso de que UserId pueda ser nulo
                Action = (string)row["Action"],
                EntityName = (string)row["EntityName"],
                EntityId = row["EntityId"] != DBNull.Value ? (int?)Convert.ToInt32(row["EntityId"]) : null, // para manejar el caso de que EntityId pueda ser nulo
                Description = (string)row["Description"],
                IpAddress = row["IpAddress"] != DBNull.Value ? (string)row["IpAddress"] : null, // para manejar el caso de que IpAddress pueda ser nulo
                CreatedAt = (DateTime)row["CreatedAt"] 
            };
            return audit;
        }
    }
}
