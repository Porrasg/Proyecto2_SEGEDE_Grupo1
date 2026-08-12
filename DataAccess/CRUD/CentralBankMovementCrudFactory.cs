using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class CentralBankMovementCrudFactory : CrudFactory
    {
        public CentralBankMovementCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }
        public override void Create(BaseDTO baseDTO)
        {
            var movement = baseDTO as CentralBankMovement;

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_CENTRALBANKMOVEMENT_PR";

            sqlOperation.AddIntParameter("CentralBankId", movement.CentralBankId);
            sqlOperation.AddStringParameter("MovementType", movement.MovementType);
            sqlOperation.AddDecimalParameter("EnergyMWh", movement.EnergyMWh);
            sqlOperation.AddDecimalParameter("InventoryAfterMovement", movement.InventoryAfterMovement);
            sqlOperation.AddDecimalParameter("SaturationLossMWh", movement.SaturationLossMWh);
            sqlOperation.AddStringParameter("Description", movement.Description);
            sqlOperation.AddDateTimeParameter("CreatedAt", movement.CreatedAt);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt",movement.UpdatedAt); // puede ser nulo

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var movement = baseDTO as CentralBankMovement;

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_CENTRALBANKMOVEMENT_PR";

            sqlOperation.AddIntParameter("MovementId", movement.Id);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();

            var operation = new SqlOperation();
            operation.ProcedureName = "RET_ALL_CENTRALBANKMOVEMENT_PR";

            var results = sqlDao.ExecuteQueryProcedure(operation);

            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var movement = BuildMovement(row);
                    list.Add((T)Convert.ChangeType(movement, typeof(T)));
                }
            }

            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var operation = new SqlOperation();
            operation.ProcedureName = "RET_BY_ID_CENTRALBANKMOVEMENT_PR";

            operation.AddIntParameter("MovementId", id);

            var results = sqlDao.ExecuteQueryProcedure(operation);

            if (results.Count > 0)
            {
                var movement = BuildMovement(results[0]);
                return (T)Convert.ChangeType(movement, typeof(T));
            }

            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var movement = baseDTO as CentralBankMovement;

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_CENTRALBANKMOVEMENT_PR";

            sqlOperation.AddIntParameter("MovementId", movement.Id);
            sqlOperation.AddIntParameter("CentralBankId", movement.CentralBankId);
            sqlOperation.AddStringParameter("MovementType", movement.MovementType);
            sqlOperation.AddDecimalParameter("EnergyMWh", movement.EnergyMWh);
            sqlOperation.AddDecimalParameter("InventoryAfterMovement", movement.InventoryAfterMovement);
            sqlOperation.AddDecimalParameter("SaturationLossMWh", movement.SaturationLossMWh);
            sqlOperation.AddStringParameter("Description", movement.Description);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", movement.UpdatedAt); // puede ser nulo

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        private CentralBankMovement BuildMovement(Dictionary<string, object> row)
        {
            return new CentralBankMovement
            {
                Id = (int)row["MovementId"],
                CentralBankId = (int)row["CentralBankId"],
                MovementType = (string)row["MovementType"],
                EnergyMWh = (decimal)row["EnergyMWh"],
                InventoryAfterMovement = (decimal)row["InventoryAfterMovement"],
                SaturationLossMWh = (decimal)row["SaturationLossMWh"],
                Description = (string)row["Description"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? (DateTime?)row["UpdatedAt"] : null // puede ser nulo
            };
        }
    }
}
