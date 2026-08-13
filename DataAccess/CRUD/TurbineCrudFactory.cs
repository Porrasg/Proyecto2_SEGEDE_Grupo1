using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class TurbineCrudFactory : CrudFactory
    {

        public TurbineCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }
        public override void Create(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Turbine
            var turbine = baseDTO as Turbine;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_TURBINE_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddStringParameter("Code", turbine.Code);
            sqlOperation.AddStringParameter("Name", turbine.Name);
            sqlOperation.AddStringParameter("Location", turbine.Location);
            sqlOperation.AddStringParameter("Brand", turbine.Brand);
            sqlOperation.AddStringParameter("Model", turbine.Model);
            sqlOperation.AddIntParameter("ManufactureYear", turbine.ManufactureYear);
            sqlOperation.AddDecimalParameter("NominalWeeklyCapacityMWh", turbine.NominalWeeklyCapacityMWh);
            sqlOperation.AddStringParameter("Status", turbine.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", turbine.CreatedAt);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", turbine.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP y agregar el id generado
            var result = sqlDao.ExecuteQueryProcedure(sqlOperation);
            
            if (result.Count > 0) 
            {
                turbine.Id = (int)result[0]["TurbineId"];
            }
        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Turbine
            var turbine = baseDTO as Turbine;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_TURBINE_PR";

            sqlOperation.AddIntParameter("TurbineId", turbine.Id);
            sqlOperation.AddStringParameter("Status", turbine.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", turbine.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todas las turbinas que se obtengan de la consulta a la BD
            var lsTurbines = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_TURBINE_PR";

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Turbine, luego agregarlo a la lista de turbinas
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var turbine = BuildTurbine(row);
                    lsTurbines.Add((T)Convert.ChangeType(turbine, typeof(T)));
                }
            }

            return lsTurbines;
        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_TURBINE_PR";

            sqlOperation.AddIntParameter("TurbineId", id);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Turbine y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var item = lsResults[0];
                var turbine = BuildTurbine(lsResults[0]);
                return (T)Convert.ChangeType(turbine, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Turbine
            var turbine = baseDTO as Turbine;

            // Definir el SP
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "UPD_TURBINE_PR";

            sqlOperation.AddIntParameter("TurbineId", turbine.Id);
            sqlOperation.AddStringParameter("Code", turbine.Code);
            sqlOperation.AddStringParameter("Name", turbine.Name);
            sqlOperation.AddStringParameter("Location", turbine.Location);
            sqlOperation.AddStringParameter("Brand", turbine.Brand);
            sqlOperation.AddStringParameter("Model", turbine.Model);
            sqlOperation.AddIntParameter("ManufactureYear", turbine.ManufactureYear);
            sqlOperation.AddDecimalParameter("NominalWeeklyCapacityMWh", turbine.NominalWeeklyCapacityMWh);
            sqlOperation.AddStringParameter("Status", turbine.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", turbine.UpdatedAt); // puede ser nulo

            //Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        // Obtiene una turbina mediante su código
        public Turbine RetrieveByCode(string code)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_CODE_TURBINE_PR";

            sqlOperation.AddStringParameter("Code", code);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Turbine y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                return BuildTurbine(lsResults[0]);
            }

            return null;
        }

        // Obtiene las turbinas que tengan el estado indicado
        public List<Turbine> RetrieveByStatus(string status)
        {
            // Lista que va a contener a todas las turbinas que se obtengan de la consulta a la BD
            var lsTurbines = new List<Turbine>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_STATUS_TURBINE_PR";

            sqlOperation.AddStringParameter("Status", status);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Turbine, luego agregarlo a la lista de turbinas
            foreach (var row in lsResults)
            {
                var turbine = BuildTurbine(row);
                lsTurbines.Add(turbine);
            }

            return lsTurbines;
        }

        //Metodo que construye el DTO del Turbine a partir de la data que viene en la consulta de la BD
        private Turbine BuildTurbine(Dictionary<string, object> row)
        {
            var turbine = new Turbine
            {
                Id = (int)row["TurbineId"],
                Code = (string)row["Code"],
                Name = (string)row["Name"],
                Location = (string)row["Location"],
                Brand = (string)row["Brand"],
                Model = (string)row["Model"],
                ManufactureYear = (int)row["ManufactureYear"],
                NominalWeeklyCapacityMWh = (decimal)row["NominalWeeklyCapacityMWh"],
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? (DateTime?)row["UpdatedAt"] : null
            };
            return turbine;
        }
    }
}
