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
            // Convirtiendo el baseDTO en un objeto Distribution
            var dist = baseDTO as Distribution;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_DISTRIBUTION_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddIntParameter("DistributionBatchId", dist.DistributionBatchId);
            sqlOperation.AddIntParameter("ForecastId", dist.ForecastId);
            sqlOperation.AddIntParameter("BuyerId", dist.BuyerId);
            sqlOperation.AddIntParameter("CentralBankId", dist.CentralBankId);
            sqlOperation.AddDecimalParameter("RequestedEnergyMWh", dist.RequestedEnergyMWh);
            sqlOperation.AddDecimalParameter("AssignedEnergyMWh", dist.AssignedEnergyMWh);
            sqlOperation.AddDecimalParameter("UnassignedEnergyMWh", dist.UnassignedEnergyMWh);
            sqlOperation.AddDecimalParameter("UnitPrice", dist.UnitPrice);
            sqlOperation.AddDateTimeParameter("DistributionDate", dist.DistributionDate);
            sqlOperation.AddStringParameter("Status", dist.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", dist.CreatedAt);

            var result = sqlDao.ExecuteQueryProcedure(sqlOperation);
            if (result.Count > 0 && result[0].TryGetValue("DistributionId", out var generatedId))
            {
                dist.Id = Convert.ToInt32(generatedId);
            }
        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Distribution
            var dist = baseDTO as Distribution;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_DISTRIBUTION_PR";

            sqlOperation.AddIntParameter("DistributionId", dist.Id);
            sqlOperation.AddStringParameter("Status", dist.Status);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todas las distribuciones que se obtengan de la consulta a la BD
            var listDistribution = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_DISTRIBUTION_PR";

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Distribution, luego agregarlo a la lista de distribuciones
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var dist = BuildDistribution(row);
                    listDistribution.Add((T)Convert.ChangeType(dist, typeof(T)));
                }
            }
            return listDistribution;
        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_DISTRIBUTION_PR";

            sqlOperation.AddIntParameter("DistributionId", id);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Distribution y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var dist = BuildDistribution(lsResults[0]);
                return (T)Convert.ChangeType(dist, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Distribution
            var dist = baseDTO as Distribution;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_DISTRIBUTION_PR";

            sqlOperation.AddIntParameter("DistributionId", dist.Id);
            sqlOperation.AddIntParameter("DistributionBatchId", dist.DistributionBatchId);
            sqlOperation.AddIntParameter("ForecastId", dist.ForecastId);
            sqlOperation.AddIntParameter("BuyerId", dist.BuyerId);
            sqlOperation.AddIntParameter("CentralBankId", dist.CentralBankId);
            sqlOperation.AddDecimalParameter("RequestedEnergyMWh", dist.RequestedEnergyMWh);
            sqlOperation.AddDecimalParameter("AssignedEnergyMWh", dist.AssignedEnergyMWh);
            sqlOperation.AddDecimalParameter("UnassignedEnergyMWh", dist.UnassignedEnergyMWh);
            sqlOperation.AddDecimalParameter("UnitPrice", dist.UnitPrice);
            sqlOperation.AddDateTimeParameter("DistributionDate", dist.DistributionDate);
            sqlOperation.AddStringParameter("Status", dist.Status);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public List<Distribution> RetrieveByBatchId( int distributionBatchId)
        {
            // Lista que va a contener a todas las distribuciones que se obtengan de la consulta a la BD
            var distributions = new List<Distribution>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_BATCH_DISTRIBUTION_PR";

            sqlOperation.AddIntParameter("DistributionBatchId", distributionBatchId);

            // Ejecutar el SP
            var results = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Distribution, luego agregarlo a la lista de distribuciones
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    distributions.Add(BuildDistribution(row));
                }
            }

            return distributions;
        }

        public List<Distribution> RetrieveByForecastId(int forecastId)
        {
            // Lista que va a contener a todas las distribuciones que se obtengan de la consulta a la BD
            var distributions = new List<Distribution>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName ="RET_BY_FORECAST_DISTRIBUTION_PR";

            sqlOperation.AddIntParameter( "ForecastId", forecastId);

            // Ejecutar el SP
            var results = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Distribution, luego agregarlo a la lista de distribuciones
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    distributions.Add(BuildDistribution(row));
                }
            }

            return distributions;
        }

        public List<Distribution> RetrieveByBuyerId(int buyerId)
        {
            // Lista que va a contener a todas las distribuciones que se obtengan de la consulta a la BD
            var distributions = new List<Distribution>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName ="RET_BY_BUYER_DISTRIBUTION_PR";

            sqlOperation.AddIntParameter("BuyerId", buyerId);

            // Ejecutar el SP
            var results = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Distribution, luego agregarlo a la lista de distribuciones
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    distributions.Add(BuildDistribution(row));
                }
            }

            return distributions;
        }

        public List<Distribution> RetrieveByCentralBankId(int centralBankId)
        {
            // Lista que va a contener a todas las distribuciones que se obtengan de la consulta a la BD
            var distributions = new List<Distribution>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_CENTRAL_BANK_DISTRIBUTION_PR";

            sqlOperation.AddIntParameter("CentralBankId",centralBankId);

            // Ejecutar el SP
            var results = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Distribution, luego agregarlo a la lista de distribuciones
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    distributions.Add(BuildDistribution(row));
                }
            }

            return distributions;
        }


        public List<Distribution> RetrieveByStatus(string status)
        {
            // Lista que va a contener a todas las distribuciones que se obtengan de la consulta a la BD
            var distributions = new List<Distribution>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_STATUS_DISTRIBUTION_PR";

            sqlOperation.AddStringParameter("Status", status);

            // Ejecutar el SP
            var results = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Distribution, luego agregarlo a la lista de distribuciones
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    distributions.Add(BuildDistribution(row));
                }
            }

            return distributions;
        }

        public List<Distribution> RetrieveByDateRange(DateTime startDate,DateTime endDate)
        {
            // Lista que va a contener a todas las distribuciones que se obtengan de la consulta a la BD
            var distributions = new List<Distribution>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_DATE_RANGE_DISTRIBUTION_PR";

            sqlOperation.AddDateTimeParameter( "StartDate", startDate);
            sqlOperation.AddDateTimeParameter( "EndDate", endDate);

            // Ejecutar el SP
            var results = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Distribution, luego agregarlo a la lista de distribuciones
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    distributions.Add(BuildDistribution(row));
                }
            }

            return distributions;
        }

        //Metodo que construye el DTO de Distribution a partir de la data que viene en la consulta de la BD 
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
