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
            var turbine = baseDTO as Turbine;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_TURBINE_PR";

            sqlOperation.AddStringParameter("Code", turbine.Code);
            sqlOperation.AddStringParameter("Name", turbine.Name);
            sqlOperation.AddStringParameter("Location", turbine.Location);
            sqlOperation.AddStringParameter("Brand", turbine.Brand);
            sqlOperation.AddStringParameter("Model", turbine.Model);
            sqlOperation.AddIntParameter("ManufactureYear", turbine.ManufactureYear);
            sqlOperation.AddDecimalParameter("NominalWeeklyCapacityMWh", turbine.NominalWeeklyCapacityMWh);
            sqlOperation.AddStringParameter("Status", turbine.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", turbine.CreatedAt);
            sqlOperation.AddDateTimeParameter("UpdatedAt", turbine.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var turbine = baseDTO as Turbine;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_TURBINE_PR";

            sqlOperation.AddIntParameter("TurbineId", turbine.Id);
            sqlOperation.AddStringParameter("Status", turbine.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", turbine.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();
            var operation = new SqlOperation();
            operation.ProcedureName = "RET_ALL_TURBINE_PR";
            var results = sqlDao.ExecuteQueryProcedure(operation);
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var turbine = BuildTurbine(row);
                    list.Add((T)Convert.ChangeType(turbine, typeof(T)));
                }
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var operation = new SqlOperation();
            operation.ProcedureName = "RET_BY_ID_TURBINE_PR";
            operation.AddIntParameter("TurbineId", id);
            var results = sqlDao.ExecuteQueryProcedure(operation);
            if (results.Count > 0)
            {
                var turbine = BuildTurbine(results[0]);
                return (T)Convert.ChangeType(turbine, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var turbine = baseDTO as Turbine;
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
            sqlOperation.AddDateTimeParameter("UpdatedAt", turbine.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

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
                UpdatedAt = (DateTime)row["UpdatedAt"]
            };
            return turbine;
        }
    }
}
