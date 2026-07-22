using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class MaintenanceCrudFactory : CrudFactory
    {

        public MaintenanceCrudFactory() {
            sqlDao = SqlDao.GetInstance();

        }

        public override void Create(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Maintenance
            var maintenance = baseDTO as Maintenance;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_MAINTENANCE_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddIntParameter("TurbineId", maintenance.TurbineId);
            sqlOperation.AddIntParameter("EngineerId", maintenance.EngineerId);
            sqlOperation.AddStringParameter("MaintenanceType", maintenance.MaintenanceType);
            sqlOperation.AddStringParameter("Description", maintenance.Description);
            sqlOperation.AddDateTimeParameter("EstimatedStartDate", maintenance.EstimatedStartDate);
            sqlOperation.AddDateTimeParameter("EstimatedEndDate", maintenance.EstimatedEndDate);
            sqlOperation.AddNullableDateTimeParameter("ActualStartDate", maintenance.ActualStartDate); // puede ser nulo si el mantenimiento aun no empieza
            sqlOperation.AddNullableDateTimeParameter("ActualEndDate", maintenance.ActualEndDate); // puede ser nulo si el mantenimiento aun no termina
            sqlOperation.AddNullableStringParameter("Result", maintenance.Result); // puede ser nulo si el mantenimiento no tiene resultado todavia
            sqlOperation.AddStringParameter("Status", maintenance.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", maintenance.CreatedAt);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", maintenance.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Maintenance
            var maintenance = baseDTO as Maintenance;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_MAINTENANCE_PR";

            sqlOperation.AddIntParameter("MaintenanceId", maintenance.Id);
            sqlOperation.AddStringParameter("Status", maintenance.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", maintenance.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todos los mantenimientos que se obtengan de la consulta a la BD
            var lsMaintenances = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_MAINTENANCE_PR";

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Maintenance, luego agregarlo a la lista de mantenimientos
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var maintenance = BuildMaintenance(row);
                    lsMaintenances.Add((T)Convert.ChangeType(maintenance, typeof(T)));
                }
            }

            return lsMaintenances;
        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_MAINTENANCE_PR";

            sqlOperation.AddIntParameter("MaintenanceId", id);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Maintenance y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var item = lsResults[0];
                var maintenance = BuildMaintenance(lsResults[0]);
                return (T)Convert.ChangeType(maintenance, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Maintenance
            var maintenance = baseDTO as Maintenance;

            // Definir el SP
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "UPD_MAINTENANCE_PR";

            sqlOperation.AddIntParameter("MaintenanceId", maintenance.Id);
            sqlOperation.AddIntParameter("TurbineId", maintenance.TurbineId);
            sqlOperation.AddIntParameter("EngineerId", maintenance.EngineerId);
            sqlOperation.AddStringParameter("MaintenanceType", maintenance.MaintenanceType);
            sqlOperation.AddStringParameter("Description", maintenance.Description);
            sqlOperation.AddDateTimeParameter("EstimatedStartDate", maintenance.EstimatedStartDate);
            sqlOperation.AddDateTimeParameter("EstimatedEndDate", maintenance.EstimatedEndDate);
            sqlOperation.AddNullableDateTimeParameter("ActualStartDate", maintenance.ActualStartDate); // puede ser nulo si el mantenimiento aun no empieza
            sqlOperation.AddNullableDateTimeParameter("ActualEndDate", maintenance.ActualEndDate); // puede ser nulo si el mantenimiento aun no termina
            sqlOperation.AddNullableStringParameter("Result", maintenance.Result); // puede ser nulo si el mantenimiento no tiene resultado todavia
            sqlOperation.AddStringParameter("Status", maintenance.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", maintenance.UpdatedAt); // puede ser nulo

            //Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        //Metodo que construye el DTO del Maintenance a partir de la data que viene en la consulta de la BD
        private Maintenance BuildMaintenance(Dictionary<string, object> row)
        {
            var maintenance = new Maintenance
            {
                Id = (int)row["MaintenanceId"],
                TurbineId = (int)row["TurbineId"],
                EngineerId = (int)row["EngineerId"],
                MaintenanceType = (string)row["MaintenanceType"],
                Description = (string)row["Description"],
                EstimatedStartDate = (DateTime)row["EstimatedStartDate"],
                EstimatedEndDate = (DateTime)row["EstimatedEndDate"],
                ActualStartDate = row["ActualStartDate"] != DBNull.Value ? (DateTime?)row["ActualStartDate"] : null, // para manejar el caso de que ActualStartDate pueda ser nulo
                ActualEndDate = row["ActualEndDate"] != DBNull.Value ? (DateTime?)row["ActualEndDate"] : null, // para manejar el caso de que ActualEndDate pueda ser nulo
                Result = row["Result"] != DBNull.Value ? (string)row["Result"] : null, // para manejar el caso de que Result pueda ser nulo
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? (DateTime?)row["UpdatedAt"] : null // para manejar el caso de que UpdatedAt pueda ser nulo
            };
            return maintenance;
        }
    }
}
