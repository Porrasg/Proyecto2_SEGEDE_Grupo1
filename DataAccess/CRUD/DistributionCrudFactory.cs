using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class DistributionCrudFactory : CrudFactory
    {
        public DistributionCrudFactory() {

            sqlDao = SqlDao.GetInstance();

        }

        public override void Create(BaseDTO baseDTO)
        {
            var d = baseDTO as Distribution;
            var sqlOperation = new SqlOperaton();
            sqlOperation.ProcedureName = "CRE_DISTRIBUTION_PR";

            sqlOperation.AddIntParameter("DistributionBatchId", d.DistributionBatchId);
            sqlOperation.AddIntParameter("ForecastId", d.ForecastId);
            sqlOperation.AddIntParameter("BuyerId", d.BuyerId);
            sqlOperation.AddIntParameter("CentralBankId", d.CentralBankId);
            sqlOperation.AddDecimalParameter("RequestedEnergyMWh", d.RequestedEnergyMWh);
            sqlOperation.AddDecimalParameter("AssignedEnergyMWh", d.AssignedEnergyMWh);
            sqlOperation.AddDecimalParameter("UnassignedEnergyMWh", d.UnassignedEnergyMWh);
            sqlOperation.AddDecimalParameter("UnitPrice", d.UnitPrice);
            sqlOperation.AddDateTimeParameter("DistributionDate", d.DistributionDate);
            sqlOperation.AddStringParameter("Status", d.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", d.CreatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var d = baseDTO as Distribution;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_DISTRIBUTION_PR";

            sqlOperation.AddIntParameter("DistributionId", d.Id);
            sqlOperation.AddStringParameter("Status", d.Status);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();
            var op = new SqlOperation();
            op.ProcedureName = "RET_ALL_DISTRIBUTION_PR";
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var dist = BuildDistribution(row);
                    list.Add((T)Convert.ChangeType(dist, typeof(T)));
                }
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var op = new SqlOperation();
            op.ProcedureName = "RET_BY_ID_DISTRIBUTION_PR";
            op.AddIntParameter("DistributionId", id);
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                var dist = BuildDistribution(results[0]);
                return (T)Convert.ChangeType(dist, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var d = baseDTO as Distribution;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_DISTRIBUTION_PR";

            sqlOperation.AddIntParameter("DistributionId", d.Id);
            sqlOperation.AddIntParameter("DistributionBatchId", d.DistributionBatchId);
            sqlOperation.AddIntParameter("ForecastId", d.ForecastId);
            sqlOperation.AddIntParameter("BuyerId", d.BuyerId);
            sqlOperation.AddIntParameter("CentralBankId", d.CentralBankId);
            sqlOperation.AddDecimalParameter("RequestedEnergyMWh", d.RequestedEnergyMWh);
            sqlOperation.AddDecimalParameter("AssignedEnergyMWh", d.AssignedEnergyMWh);
            sqlOperation.AddDecimalParameter("UnassignedEnergyMWh", d.UnassignedEnergyMWh);
            sqlOperation.AddDecimalParameter("UnitPrice", d.UnitPrice);
            sqlOperation.AddDateTimeParameter("DistributionDate", d.DistributionDate);
            sqlOperation.AddStringParameter("Status", d.Status);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        private Distribution BuildDistribution(Dictionary<string, object> row)
        {
            var dist = new Distribution
            {
                Id = (int)row["DistributionId"],
                DistributionBatchId = (int)row["DistributionBatchId"],
                ForecastId = (int)row["ForecastId"],
                BuyerId = (int)row["BuyerId"],
                CentralBankId = (int)row["CentralBankId"],
                RequestedEnergyMWh = (decimal)row["RequestedEnergyMWh"],
                AssignedEnergyMWh = (decimal)row["AssignedEnergyMWh"],
                UnassignedEnergyMWh = (decimal)row["UnassignedEnergyMWh"],
                UnitPrice = (decimal)row["UnitPrice"],
                DistributionDate = (DateTime)row["DistributionDate"],
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"]
            };
            return dist;
        }
    }
}
