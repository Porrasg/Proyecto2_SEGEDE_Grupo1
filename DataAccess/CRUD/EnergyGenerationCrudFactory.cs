using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class EnergyGenerationCrudFactory : CrudFactory
    {

        public EnergyGenerationCrudFactory() 
        {
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            throw new NotImplementedException();
        }

        public override void Delete(BaseDTO baseDTO)
        {
            throw new NotImplementedException();
        }

        public override List<T> RetrieveAll<T>()
        {
            throw new NotImplementedException();
        }

        public override T RetrieveById<T>(int id)
        {
            throw new NotImplementedException();
        }

        public override void Update(BaseDTO baseDTO)
        {
            throw new NotImplementedException();
        }


        // Implementación del método RetrieveByTurbine para obtener la generación de energía por turbina
        public List<EnergyGeneration> RetrieveByTurbine(int turbineId)
        {

            var generationList = new List<EnergyGeneration>();

            //Definir el StoreProcedure para obtener la generación de energía por turbina
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_TURBINE_ENERGY_GENERATION_PR";

            sqlOperation.AddIntParameter("TurbineId", turbineId);

            //Ejecutar el StoreProcedure y obtener los resultados
            var listResult = sqlDao.ExecuteQueryProcedure(sqlOperation);

            //Mapear los resultados a objetos EnergyGeneration
            if (listResult.Count > 0)
            {
                foreach (var row in listResult) 
                {
                    var generation = BuildEnergyGeneration(row);
                    generationList.Add(generation);
                }
            }
            return generationList;
        }


        // Método privado para mapear los resultados de la consulta a objetos EnergyGeneration (DTO)
        private EnergyGeneration BuildEnergyGeneration(Dictionary<string, object> row)
        {
            var generation = new EnergyGeneration
            {
                Id = (int)row["EnergyGenerationId"],
                TurbineId = (int)row["TurbineId"],
                GenerateMWh = (decimal)row["GenerateMWh"],
                WindSpeedMs = (decimal)row["WindSpeedMs"],
                GenerateAt = (DateTime)row["GenerateAt"],
                CreateAt = (DateTime)row["CreateAt"]
            };
            
            return generation;
        }

    }
}
