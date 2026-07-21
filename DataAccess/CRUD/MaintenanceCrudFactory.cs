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
            var maintenance = baseDTO as Maintenance;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_MAINTENANCE_PR";

            sqlOperation.AddIntParameter("TurbineId", maintenance.TurbineId);
            sqlOperation.AddIntParameter("EngineerId", maintenance.EngineerId);
            sqlOperation.AddStringParameter("MaintenanceType", maintenance.MaintenanceType);
            sqlOperation.AddStringParameter("Description", maintenance.Description);
            sqlOperation.AddDateTimeParameter("EstimatedStartDate", maintenance.EstimatedStartDate);
            sqlOperation.AddDateTimeParameter("EstimatedEndDate", maintenance.EstimatedEndDate);
            sqlOperation.AddDateTimeParameter("ActualStartDate", maintenance.ActualStartDate ?? default(DateTime));
            sqlOperation.AddDateTimeParameter("ActualEndDate", maintenance.ActualEndDate ?? default(DateTime));
            sqlOperation.AddStringParameter("Result", maintenance.Result);
            sqlOperation.AddStringParameter("Status", maintenance.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", maintenance.CreatedAt);
            sqlOperation.AddDateTimeParameter("UpdatedAt", maintenance.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var maintenance = baseDTO as Maintenance;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_MAINTENANCE_PR";

            sqlOperation.AddIntParameter("MaintenanceId", maintenance.Id);
            sqlOperation.AddStringParameter("Status", maintenance.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", maintenance.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();
            var operation = new SqlOperation();
            operation.ProcedureName = "RET_ALL_MAINTENANCE_PR";
            var results = sqlDao.ExecuteQueryProcedure(operation);
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var m = BuildMaintenance(row);
                    list.Add((T)Convert.ChangeType(m, typeof(T)));
                }
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var op = new SqlOperation();
            op.ProcedureName = "RET_BY_ID_MAINTENANCE_PR";
            op.AddIntParameter("MaintenanceId", id);
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                var m = BuildMaintenance(results[0]);
                return (T)Convert.ChangeType(m, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var maintenance = baseDTO as Maintenance;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_MAINTENANCE_PR";

            sqlOperation.AddIntParameter("MaintenanceId", maintenance.Id);
            sqlOperation.AddIntParameter("TurbineId", maintenance.TurbineId);
            sqlOperation.AddIntParameter("EngineerId", maintenance.EngineerId);
            sqlOperation.AddStringParameter("MaintenanceType", maintenance.MaintenanceType);
            sqlOperation.AddStringParameter("Description", maintenance.Description);
            sqlOperation.AddDateTimeParameter("EstimatedStartDate", maintenance.EstimatedStartDate);
            sqlOperation.AddDateTimeParameter("EstimatedEndDate", maintenance.EstimatedEndDate);
            sqlOperation.AddDateTimeParameter("ActualStartDate", maintenance.ActualStartDate ?? default(DateTime));
            sqlOperation.AddDateTimeParameter("ActualEndDate", maintenance.ActualEndDate ?? default(DateTime));
            sqlOperation.AddStringParameter("Result", maintenance.Result);
            sqlOperation.AddStringParameter("Status", maintenance.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", maintenance.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        private Maintenance BuildMaintenance(Dictionary<string, object> row)
        {
            var m = new Maintenance
            {
                Id = (int)row["MaintenanceId"],
                TurbineId = (int)row["TurbineId"],
                EngineerId = (int)row["EngineerId"],
                MaintenanceType = (string)row["MaintenanceType"],
                Description = (string)row["Description"],
                EstimatedStartDate = (DateTime)row["EstimatedStartDate"],
                EstimatedEndDate = (DateTime)row["EstimatedEndDate"],
                ActualStartDate = row.ContainsKey("ActualStartDate") && row["ActualStartDate"] != DBNull.Value ? (DateTime?)row["ActualStartDate"] : null,
                ActualEndDate = row.ContainsKey("ActualEndDate") && row["ActualEndDate"] != DBNull.Value ? (DateTime?)row["ActualEndDate"] : null,
                Result = row.ContainsKey("Result") && row["Result"] != DBNull.Value ? row["Result"].ToString() : null,
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = (DateTime)row["UpdatedAt"]
            };
            return m;
        }
    }
}
