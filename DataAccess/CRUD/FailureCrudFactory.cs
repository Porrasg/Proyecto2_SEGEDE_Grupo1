using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class FailureCrudFactory : CrudFactory
    {
        public FailureCrudFactory() {

            sqlDao = SqlDao.GetInstance();

        }

        public override void Create(BaseDTO baseDTO)
        {
            var failure = baseDTO as Failure;
            var sqlOperation = new SqlOperaton();
            sqlOperation.ProcedureName = "CRE_FAILURE_PR";

            sqlOperation.AddIntParameter("TurbineId", failure.TurbineId);
            sqlOperation.AddIntParameter("EngineerId", failure.EngineerId);
            sqlOperation.AddDateTimeParameter("FailureDate", failure.FailureDate);
            sqlOperation.AddStringParameter("Severity", failure.Severity);
            sqlOperation.AddStringParameter("Description", failure.Description);
            sqlOperation.AddStringParameter("Resolution", failure.Resolution);
            sqlOperation.AddStringParameter("Status", failure.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", failure.CreatedAt);
            sqlOperation.AddDateTimeParameter("UpdatedAt", failure.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var failure = baseDTO as Failure;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_FAILURE_PR";

            sqlOperation.AddIntParameter("FailureId", failure.Id);
            sqlOperation.AddStringParameter("Status", failure.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", failure.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();
            var operation = new SqlOperation();
            operation.ProcedureName = "RET_ALL_FAILURE_PR";
            var results = sqlDao.ExecuteQueryProcedure(operation);
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var f = BuildFailure(row);
                    list.Add((T)Convert.ChangeType(f, typeof(T)));
                }
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var op = new SqlOperation();
            op.ProcedureName = "RET_BY_ID_FAILURE_PR";
            op.AddIntParameter("FailureId", id);
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                var f = BuildFailure(results[0]);
                return (T)Convert.ChangeType(f, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var failure = baseDTO as Failure;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_FAILURE_PR";

            sqlOperation.AddIntParameter("FailureId", failure.Id);
            sqlOperation.AddIntParameter("TurbineId", failure.TurbineId);
            sqlOperation.AddIntParameter("EngineerId", failure.EngineerId);
            sqlOperation.AddDateTimeParameter("FailureDate", failure.FailureDate);
            sqlOperation.AddStringParameter("Severity", failure.Severity);
            sqlOperation.AddStringParameter("Description", failure.Description);
            sqlOperation.AddStringParameter("Resolution", failure.Resolution);
            sqlOperation.AddStringParameter("Status", failure.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", failure.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        private Failure BuildFailure(Dictionary<string, object> row)
        {
            var f = new Failure
            {
                Id = (int)row["FailureId"],
                TurbineId = (int)row["TurbineId"],
                EngineerId = (int)row["EngineerId"],
                FailureDate = (DateTime)row["FailureDate"],
                Severity = (string)row["Severity"],
                Description = (string)row["Description"],
                Resolution = row.ContainsKey("Resolution") && row["Resolution"] != DBNull.Value ? row["Resolution"].ToString() : null,
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = (DateTime)row["UpdatedAt"]
            };
            return f;
        }
    }
}
