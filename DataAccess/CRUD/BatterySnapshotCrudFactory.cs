using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;

namespace DataAccess.CRUD
{
    public class BatterySnapshotCrudFactory : CrudFactory
    {
        public BatterySnapshotCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            throw new NotSupportedException("Las capturas de batería se crean automáticamente durante un Flush");
        }

        public override void Update(BaseDTO baseDTO)
        {
            throw new NotSupportedException("Las capturas de batería son históricas y no se modifican");
        }

        public override void Delete(BaseDTO baseDTO)
        {
            throw new NotSupportedException("Las capturas de batería son históricas y no se eliminan");
        }

        public override T RetrieveById<T>(int id)
        {
            throw new NotSupportedException("Este módulo solo expone el historial completo de capturas");
        }

        public override List<T> RetrieveAll<T>()
        {
            var snapshots = new List<T>();

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName =
                "RET_ALL_BATTERY_SNAPSHOTS_PR";

            var results =
                sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var snapshot = BuildBatterySnapshot(row);

                    snapshots.Add(
                        (T)Convert.ChangeType(
                            snapshot,
                            typeof(T)
                        )
                    );
                }
            }

            return snapshots;
        }

        private BatterySnapshot BuildBatterySnapshot(
            Dictionary<string, object> row)
        {
            var snapshot = new BatterySnapshot
            {
                Id = Convert.ToInt32(row["SnapshotId"]),

                FlushId = Convert.ToInt32(row["FlushId"]),
                BatteryId = Convert.ToInt32(row["BatteryId"]),
                TurbineId = Convert.ToInt32(row["TurbineId"]),

                MaximumCapacityMWh =
                    Convert.ToDecimal(row["MaximumCapacityMWh"]),

                CurrentEnergyMWh =
                    Convert.ToDecimal(row["CurrentEnergyMWh"]),

                TotalGeneratedMWh =
                    Convert.ToDecimal(row["TotalGeneratedMWh"]),

                TotalTransferredMWh =
                    Convert.ToDecimal(row["TotalTransferredMWh"]),

                TotalSaturationLossMWh =
                    Convert.ToDecimal(row["TotalSaturationLossMWh"]),

                Status =
                    row["Status"].ToString(),

                CapturedAt =
                    Convert.ToDateTime(row["CapturedAt"])
            };

            return snapshot;
        }
    }
}
