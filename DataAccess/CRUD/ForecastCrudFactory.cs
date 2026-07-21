using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class ForecastCrudFactory : CrudFactory
    {
        public ForecastCrudFactory() {

            sqlDao = SqlDao.GetInstance();

        }

        public override void Create(BaseDTO baseDTO)
        {
            var fcast = baseDTO as Forecast;
            var sqlOperation = new SqlOperaton();
            sqlOperation.ProcedureName = "CRE_FORECAST_PR";

            sqlOperation.AddIntParameter("BuyerId", fcast.BuyerId);
            sqlOperation.AddIntParameter("ForecastYear", fcast.ForecastYear);
            sqlOperation.AddIntParameter("ForecastMonth", fcast.ForecastMonth);
            sqlOperation.AddDecimalParameter("RequestedEnergyMWh", fcast.RequestedEnergyMWh);
            sqlOperation.AddStringParameter("Status", fcast.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", fcast.CreatedAt);
            sqlOperation.AddDateTimeParameter("UpdatedAt", fcast.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var fcast = baseDTO as Forecast;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_FORECAST_PR";

            sqlOperation.AddIntParameter("ForecastId", fcast.Id);
            sqlOperation.AddStringParameter("Status", fcast.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", fcast.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();
            var op = new SqlOperation();
            op.ProcedureName = "RET_ALL_FORECAST_PR";
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var f = BuildForecast(row);
                    list.Add((T)Convert.ChangeType(f, typeof(T)));
                }
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var op = new SqlOperation();
            op.ProcedureName = "RET_BY_ID_FORECAST_PR";
            op.AddIntParameter("ForecastId", id);
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                var f = BuildForecast(results[0]);
                return (T)Convert.ChangeType(f, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var fcast = baseDTO as Forecast;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_FORECAST_PR";

            sqlOperation.AddIntParameter("ForecastId", fcast.Id);
            sqlOperation.AddIntParameter("BuyerId", fcast.BuyerId);
            sqlOperation.AddIntParameter("ForecastYear", fcast.ForecastYear);
            sqlOperation.AddIntParameter("ForecastMonth", fcast.ForecastMonth);
            sqlOperation.AddDecimalParameter("RequestedEnergyMWh", fcast.RequestedEnergyMWh);
            sqlOperation.AddStringParameter("Status", fcast.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", fcast.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        private Forecast BuildForecast(Dictionary<string, object> row)
        {
            var f = new Forecast
            {
                Id = (int)row["ForecastId"],
                BuyerId = (int)row["BuyerId"],
                ForecastYear = (int)row["ForecastYear"],
                ForecastMonth = (int)row["ForecastMonth"],
                RequestedEnergyMWh = (decimal)row["RequestedEnergyMWh"],
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = (DateTime)row["UpdatedAt"]
            };
            return f;
        }
    }
}
