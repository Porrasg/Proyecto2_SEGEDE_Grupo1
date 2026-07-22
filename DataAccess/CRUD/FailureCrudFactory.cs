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
            // Convirtiendo el baseDTO en un objeto Failure
            var failure = baseDTO as Failure;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_FAILURE_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddIntParameter("TurbineId", failure.TurbineId);
            sqlOperation.AddIntParameter("EngineerId", failure.EngineerId);
            sqlOperation.AddDateTimeParameter("FailureDate", failure.FailureDate);
            sqlOperation.AddStringParameter("Severity", failure.Severity);
            sqlOperation.AddStringParameter("Description", failure.Description);
            sqlOperation.AddNullableStringParameter("Resolution", failure.Resolution); // puede ser nulo si la falla aun no se ha resuelto
            sqlOperation.AddStringParameter("Status", failure.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", failure.CreatedAt);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", failure.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Failure
            var failure = baseDTO as Failure;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_FAILURE_PR";

            sqlOperation.AddIntParameter("FailureId", failure.Id);
            sqlOperation.AddStringParameter("Status", failure.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", failure.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todas las fallas que se obtengan de la consulta a la BD
            var lsFailures = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_FAILURE_PR";

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Failure, luego agregarlo a la lista de fallas
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var failure = BuildFailure(row);
                    lsFailures.Add((T)Convert.ChangeType(failure, typeof(T)));
                }
            }

            return lsFailures;
        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_FAILURE_PR";

            sqlOperation.AddIntParameter("FailureId", id);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Failure y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var item = lsResults[0];
                var failure = BuildFailure(lsResults[0]);
                return (T)Convert.ChangeType(failure, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Failure
            var failure = baseDTO as Failure;

            // Definir el SP
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "UPD_FAILURE_PR";

            sqlOperation.AddIntParameter("FailureId", failure.Id);
            sqlOperation.AddIntParameter("TurbineId", failure.TurbineId);
            sqlOperation.AddIntParameter("EngineerId", failure.EngineerId);
            sqlOperation.AddDateTimeParameter("FailureDate", failure.FailureDate);
            sqlOperation.AddStringParameter("Severity", failure.Severity);
            sqlOperation.AddStringParameter("Description", failure.Description);
            sqlOperation.AddNullableStringParameter("Resolution", failure.Resolution); // puede ser nulo si la falla aun no se ha resuelto
            sqlOperation.AddStringParameter("Status", failure.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", failure.UpdatedAt); // puede ser nulo

            //Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public List<Failure> RetrieveByTurbineId(int turbineId)
        {
            // Lista que va a contener las fallas asociadas a la turbina
            var lsFailures = new List<Failure>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_TURBINE_FAILURE_PR";

            sqlOperation.AddIntParameter("TurbineId", turbineId);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer los resultados y construir los objetos Failure
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var failure = BuildFailure(row);
                    lsFailures.Add(failure);
                }
            }

            return lsFailures;
        }

        public List<Failure> RetrieveByEngineerId(int engineerId)
        {
            // Lista que va a contener las fallas asociadas al ingeniero
            var lsFailures = new List<Failure>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ENGINEER_FAILURE_PR";

            sqlOperation.AddIntParameter("EngineerId", engineerId);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer los resultados y construir los objetos Failure
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var failure = BuildFailure(row);
                    lsFailures.Add(failure);
                }
            }

            return lsFailures;
        }

        public List<Failure> RetrieveByStatus(string status)
        {
            // Lista que va a contener las fallas con el estado indicado
            var lsFailures = new List<Failure>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_STATUS_FAILURE_PR";

            sqlOperation.AddStringParameter("Status", status);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer los resultados y construir los objetos Failure
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var failure = BuildFailure(row);
                    lsFailures.Add(failure);
                }
            }

            return lsFailures;
        }

        public List<Failure> RetrieveBySeverity(string severity)
        {
            // Lista que va a contener las fallas con la severidad indicada
            var lsFailures = new List<Failure>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_SEVERITY_FAILURE_PR";

            sqlOperation.AddStringParameter("Severity", severity);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer los resultados y construir los objetos Failure
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var failure = BuildFailure(row);
                    lsFailures.Add(failure);
                }
            }

            return lsFailures;
        }

        //Metodo que construye el DTO del Failure a partir de la data que viene en la consulta de la BD
        private Failure BuildFailure(Dictionary<string, object> row)
        {
            var failure = new Failure
            {
                Id = (int)row["FailureId"],
                TurbineId = (int)row["TurbineId"],
                EngineerId = (int)row["EngineerId"],
                FailureDate = (DateTime)row["FailureDate"],
                Severity = (string)row["Severity"],
                Description = (string)row["Description"],
                Resolution = row["Resolution"] != DBNull.Value ? (string)row["Resolution"] : null, // para manejar el caso de que Resolution pueda ser nulo
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? (DateTime)row["UpdatedAt"] : null // para manejar el caso de que UpdatedAt pueda ser nulo
            };
            return failure;
        }
    }
}