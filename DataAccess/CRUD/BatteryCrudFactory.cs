using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class BatteryCrudFactory : CrudFactory
    {
        public BatteryCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var battery = baseDTO as Battery;
            var sqlOperation = new SqlOperaton();
            sqlOperation.ProcedureName = "CRE_BATTERY_PR";

            sqlOperation.AddIntParameter("TurbineId", battery.TurbineId);
            sqlOperation.AddDecimalParameter("MaximumCapacityMWh", battery.MaximumCapacityMWh);
            sqlOperation.AddDecimalParameter("CurrentEnergyMWh", battery.CurrentEnergyMWh);
            sqlOperation.AddDecimalParameter("TotalGeneratedMWh", battery.TotalGeneratedMWh);
            sqlOperation.AddDecimalParameter("TotalTransferredMWh", battery.TotalTransferredMWh);
            sqlOperation.AddDecimalParameter("TotalSaturationLossMWh", battery.TotalSaturationLossMWh);
            sqlOperation.AddStringParameter("Status", battery.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", battery.CreatedAt);
            sqlOperation.AddDateTimeParameter("UpdatedAt", battery.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var battery = baseDTO as Battery;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_BATTERY_PR";

            sqlOperation.AddIntParameter("BatteryId", battery.Id);
            sqlOperation.AddStringParameter("Status", battery.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", battery.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();
            var op = new SqlOperation();
            op.ProcedureName = "RET_ALL_BATTERY_PR";
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var b = BuildBattery(row);
                    list.Add((T)Convert.ChangeType(b, typeof(T)));
                }
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var op = new SqlOperation();
            op.ProcedureName = "RET_BY_ID_BATTERY_PR";
            op.AddIntParameter("BatteryId", id);
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                var b = BuildBattery(results[0]);
                return (T)Convert.ChangeType(b, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var battery = baseDTO as Battery;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_BATTERY_PR";

            sqlOperation.AddIntParameter("BatteryId", battery.Id);
            sqlOperation.AddIntParameter("TurbineId", battery.TurbineId);
            sqlOperation.AddDecimalParameter("MaximumCapacityMWh", battery.MaximumCapacityMWh);
            sqlOperation.AddDecimalParameter("CurrentEnergyMWh", battery.CurrentEnergyMWh);
            sqlOperation.AddDecimalParameter("TotalGeneratedMWh", battery.TotalGeneratedMWh);
            sqlOperation.AddDecimalParameter("TotalTransferredMWh", battery.TotalTransferredMWh);
            sqlOperation.AddDecimalParameter("TotalSaturationLossMWh", battery.TotalSaturationLossMWh);
            sqlOperation.AddStringParameter("Status", battery.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", battery.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        private Battery BuildBattery(Dictionary<string, object> row)
        {
            var b = new Battery
            {
                Id = (int)row["BatteryId"],
                TurbineId = (int)row["TurbineId"],
                MaximumCapacityMWh = (decimal)row["MaximumCapacityMWh"],
                CurrentEnergyMWh = (decimal)row["CurrentEnergyMWh"],
                TotalGeneratedMWh = (decimal)row["TotalGeneratedMWh"],
                TotalTransferredMWh = (decimal)row["TotalTransferredMWh"],
                TotalSaturationLossMWh = (decimal)row["TotalSaturationLossMWh"],
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = (DateTime)row["UpdatedAt"]
            };
            return b;
        }
    }
}
