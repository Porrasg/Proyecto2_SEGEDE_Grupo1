using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class CentralBankCrudFactory : CrudFactory
    {
        public CentralBankCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var cb = baseDTO as CentralBank;
            var sqlOperation = new SqlOperaton();
            sqlOperation.ProcedureName = "CRE_CENTRAL_BANK_PR";

            sqlOperation.AddStringParameter("Name", cb.Name);
            sqlOperation.AddDecimalParameter("MaximumCapacityMWh", cb.MaximumCapacityMWh);
            sqlOperation.AddDecimalParameter("CurrentInventoryMWh", cb.CurrentInventoryMWh);
            sqlOperation.AddDecimalParameter("TotalReceivedMWh", cb.TotalReceivedMWh);
            sqlOperation.AddDecimalParameter("TotalDistributedMWh", cb.TotalDistributedMWh);
            sqlOperation.AddDecimalParameter("TotalSaturationLossMWh", cb.TotalSaturationLossMWh);
            sqlOperation.AddStringParameter("Status", cb.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", cb.CreatedAt);
            sqlOperation.AddDateTimeParameter("UpdatedAt", cb.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var cb = baseDTO as CentralBank;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_CENTRAL_BANK_PR";

            sqlOperation.AddIntParameter("CentralBankId", cb.Id);
            sqlOperation.AddStringParameter("Status", cb.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", cb.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();
            var op = new SqlOperation();
            op.ProcedureName = "RET_ALL_CENTRAL_BANK_PR";
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var c = BuildCentralBank(row);
                    list.Add((T)Convert.ChangeType(c, typeof(T)));
                }
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var op = new SqlOperation();
            op.ProcedureName = "RET_BY_ID_CENTRAL_BANK_PR";
            op.AddIntParameter("CentralBankId", id);
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                var c = BuildCentralBank(results[0]);
                return (T)Convert.ChangeType(c, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var cb = baseDTO as CentralBank;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_CENTRAL_BANK_PR";

            sqlOperation.AddIntParameter("CentralBankId", cb.Id);
            sqlOperation.AddStringParameter("Name", cb.Name);
            sqlOperation.AddDecimalParameter("MaximumCapacityMWh", cb.MaximumCapacityMWh);
            sqlOperation.AddDecimalParameter("CurrentInventoryMWh", cb.CurrentInventoryMWh);
            sqlOperation.AddDecimalParameter("TotalReceivedMWh", cb.TotalReceivedMWh);
            sqlOperation.AddDecimalParameter("TotalDistributedMWh", cb.TotalDistributedMWh);
            sqlOperation.AddDecimalParameter("TotalSaturationLossMWh", cb.TotalSaturationLossMWh);
            sqlOperation.AddStringParameter("Status", cb.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", cb.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        private CentralBank BuildCentralBank(Dictionary<string, object> row)
        {
            var c = new CentralBank
            {
                Id = (int)row["CentralBankId"],
                Name = (string)row["Name"],
                MaximumCapacityMWh = (decimal)row["MaximumCapacityMWh"],
                CurrentInventoryMWh = (decimal)row["CurrentInventoryMWh"],
                TotalReceivedMWh = (decimal)row["TotalReceivedMWh"],
                TotalDistributedMWh = (decimal)row["TotalDistributedMWh"],
                TotalSaturationLossMWh = (decimal)row["TotalSaturationLossMWh"],
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = (DateTime)row["UpdatedAt"]
            };
            return c;
        }
    }
}
