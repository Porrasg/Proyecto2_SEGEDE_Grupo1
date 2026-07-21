using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class FlushCrudFactory : CrudFactory
    {
        public FlushCrudFactory() 
        {
            sqlDao = SqlDao.GetInstance();

        }

        public override void Create(BaseDTO baseDTO)
        {
            var flush = baseDTO as Flush;
            var sqlOperation = new SqlOperaton();
            sqlOperation.ProcedureName = "CRE_FLUSH_PR";

            sqlOperation.AddIntParameter("FlushBatchId", flush.FlushBatchId);
            sqlOperation.AddIntParameter("TurbineId", flush.TurbineId);
            sqlOperation.AddIntParameter("BatteryId", flush.BatteryId);
            sqlOperation.AddIntParameter("CentralBankId", flush.CentralBankId);
            sqlOperation.AddDecimalParameter("SnapshotEnergyMWh", flush.SnapshotEnergyMWh);
            sqlOperation.AddDecimalParameter("TransferredEnergyMWh", flush.TransferredEnergyMWh);
            sqlOperation.AddDecimalParameter("SaturationLossMWh", flush.SaturationLossMWh);
            sqlOperation.AddStringParameter("ExecutionType", flush.ExecutionType);
            sqlOperation.AddStringParameter("Status", flush.Status);
            sqlOperation.AddDateTimeParameter("ExecutedAt", flush.ExecutedAt);
            sqlOperation.AddDateTimeParameter("CreatedAt", flush.CreatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var flush = baseDTO as Flush;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_FLUSH_PR";

            sqlOperation.AddIntParameter("FlushId", flush.Id);
            sqlOperation.AddStringParameter("Status", flush.Status);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();
            var op = new SqlOperation();
            op.ProcedureName = "RET_ALL_FLUSH_PR";
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var f = BuildFlush(row);
                    list.Add((T)Convert.ChangeType(f, typeof(T)));
                }
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var op = new SqlOperation();
            op.ProcedureName = "RET_BY_ID_FLUSH_PR";
            op.AddIntParameter("FlushId", id);
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                var f = BuildFlush(results[0]);
                return (T)Convert.ChangeType(f, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var flush = baseDTO as Flush;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_FLUSH_PR";

            sqlOperation.AddIntParameter("FlushId", flush.Id);
            sqlOperation.AddIntParameter("FlushBatchId", flush.FlushBatchId);
            sqlOperation.AddIntParameter("TurbineId", flush.TurbineId);
            sqlOperation.AddIntParameter("BatteryId", flush.BatteryId);
            sqlOperation.AddIntParameter("CentralBankId", flush.CentralBankId);
            sqlOperation.AddDecimalParameter("SnapshotEnergyMWh", flush.SnapshotEnergyMWh);
            sqlOperation.AddDecimalParameter("TransferredEnergyMWh", flush.TransferredEnergyMWh);
            sqlOperation.AddDecimalParameter("SaturationLossMWh", flush.SaturationLossMWh);
            sqlOperation.AddStringParameter("ExecutionType", flush.ExecutionType);
            sqlOperation.AddStringParameter("Status", flush.Status);
            sqlOperation.AddDateTimeParameter("ExecutedAt", flush.ExecutedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        private Flush BuildFlush(Dictionary<string, object> row)
        {
            var f = new Flush
            {
                Id = (int)row["FlushId"],
                FlushBatchId = (int)row["FlushBatchId"],
                TurbineId = (int)row["TurbineId"],
                BatteryId = (int)row["BatteryId"],
                CentralBankId = (int)row["CentralBankId"],
                SnapshotEnergyMWh = (decimal)row["SnapshotEnergyMWh"],
                TransferredEnergyMWh = (decimal)row["TransferredEnergyMWh"],
                SaturationLossMWh = (decimal)row["SaturationLossMWh"],
                ExecutionType = (string)row["ExecutionType"],
                Status = (string)row["Status"],
                ExecutedAt = (DateTime)row["ExecutedAt"],
                CreatedAt = (DateTime)row["CreatedAt"]
            };
            return f;
        }
    }
}
