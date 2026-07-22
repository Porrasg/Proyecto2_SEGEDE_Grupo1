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
            // Convirtiendo el baseDTO en un objeto Forecast
            var forecast = baseDTO as Forecast;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_FORECAST_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddIntParameter("BuyerId", forecast.BuyerId);
            sqlOperation.AddIntParameter("ForecastYear", forecast.ForecastYear);
            sqlOperation.AddIntParameter("ForecastMonth", forecast.ForecastMonth);
            sqlOperation.AddDecimalParameter("RequestedEnergyMWh", forecast.RequestedEnergyMWh);
            sqlOperation.AddStringParameter("Status", forecast.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", forecast.CreatedAt);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", forecast.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Forecast
            var forecast = baseDTO as Forecast;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_FORECAST_PR";

            sqlOperation.AddIntParameter("ForecastId", forecast.Id);
            sqlOperation.AddStringParameter("Status", forecast.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", forecast.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todos los forecasts que se obtengan de la consulta a la BD
            var lsForecasts = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_FORECAST_PR";

            //  Ejecutamos el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Forecast, luego agregarlo a la lista de forecasts
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var forecast = BuildForecast(row);
                    lsForecasts.Add((T)Convert.ChangeType(forecast, typeof(T)));
                }
            }
            return lsForecasts;
        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_FORECAST_PR";

            sqlOperation.AddIntParameter("ForecastId", id);

            // Ejecutamos el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Forecast y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var forecast = BuildForecast(lsResults[0]);
                return (T)Convert.ChangeType(forecast, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Forecast
            var forecast = baseDTO as Forecast;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_FORECAST_PR";

            sqlOperation.AddIntParameter("ForecastId", forecast.Id);
            sqlOperation.AddIntParameter("BuyerId", forecast.BuyerId);
            sqlOperation.AddIntParameter("ForecastYear", forecast.ForecastYear);
            sqlOperation.AddIntParameter("ForecastMonth", forecast.ForecastMonth);
            sqlOperation.AddDecimalParameter("RequestedEnergyMWh", forecast.RequestedEnergyMWh);
            sqlOperation.AddStringParameter("Status", forecast.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", forecast.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public List<Forecast> RetrieveByBuyerId(int buyerId)
        {
            var forecasts = new List<Forecast>();

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_BUYER_FORECAST_PR";

            sqlOperation.AddIntParameter("BuyerId", buyerId);

            var results = sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    forecasts.Add(BuildForecast(row));
                }
            }

            return forecasts;
        }

        // Metodo que permite obtener todos los forecasts de un periodo especifico (año y mes)
        public List<Forecast> RetrieveByPeriod(int forecastYear,int forecastMonth)
        {
            // Lista que va a contener a todos los forecasts que se obtengan de la consulta a la BD
            var forecasts = new List<Forecast>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_PERIOD_FORECAST_PR";

            sqlOperation.AddIntParameter("ForecastYear", forecastYear);
            sqlOperation.AddIntParameter("ForecastMonth", forecastMonth);

            // Ejecutamos el SP
            var results = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Forecast, luego agregarlo a la lista de forecasts
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    forecasts.Add(BuildForecast(row));
                }
            }

            return forecasts;
        }

        // Metodo que permite obtener un forecast de un comprador especifico en un periodo especifico (año y mes)
        public Forecast RetrieveByBuyerPeriod(int buyerId, int forecastYear, int forecastMonth)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_BUYER_PERIOD_FORECAST_PR";

            sqlOperation.AddIntParameter("BuyerId", buyerId);
            sqlOperation.AddIntParameter("ForecastYear", forecastYear);
            sqlOperation.AddIntParameter("ForecastMonth", forecastMonth);

            // Ejecutamos el SP
            var results = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Forecast y devolverlo, de lo contrario devolver null
            if (results.Count > 0)
            {
                return BuildForecast(results[0]);
            }

            return null;
        }

        // Metodo que permite obtener todos los forecasts de un estado especifico
        public List<Forecast> RetrieveByStatus(string status)
        {
            // Lista que va a contener a todos los forecasts que se obtengan de la consulta a la BD
            var forecasts = new List<Forecast>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_STATUS_FORECAST_PR";

            sqlOperation.AddStringParameter("Status", status);

            // Ejecutamos el SP
            var results = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Forecast, luego agregarlo a la lista de forecasts
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    forecasts.Add(BuildForecast(row));
                }
            }

            return forecasts;
        }

        // Metodo que construye el DTO Forecast a partir de la data que viene en la consulta de la BD
        private Forecast BuildForecast(Dictionary<string, object> row)
        {
            var forecast = new Forecast
            {
                Id = (int)row["ForecastId"],
                BuyerId = (int)row["BuyerId"],
                ForecastYear = (int)row["ForecastYear"],
                ForecastMonth = (int)row["ForecastMonth"],
                RequestedEnergyMWh = (decimal)row["RequestedEnergyMWh"],
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? (DateTime?)row["UpdatedAt"] : null
            };
            return forecast;
        }
    }
}
