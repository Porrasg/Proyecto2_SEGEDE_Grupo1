using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class EnergyLossCrudFactory : CrudFactory
    {
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


        public List<EnergyLoss> RetrieveByTurbine(int turbineId)
        { 
            var lossList = new List<EnergyLoss>();

            //Definir el store procedure para obtener las pérdidas de energía por turbina
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_TURBINE_ENERGY_LOSS_PR";
            sqlOperation.AddIntParameter("TurbineId", turbineId);

            //Ejecutar el store procedure y obtener los resultados
            var listResult = sqlDao.ExecuteQueryProcedure(sqlOperation);

            //Construir los DTOs a partir de los resultados obtenidos
            if (listResult.Count > 0) 
            {
                foreach (var row in listResult) 
                {
                    var loss = BuildEnergyLoss(row);
                    lossList.Add(loss);
                }
            }

            return lossList;
        }


        // Construir un objeto EnergyLoss a partir de un diccionario de resultados obtenidos de la base de datos
        private EnergyLoss BuildEnergyLoss(Dictionary<string, object> row)
        {
            var loss = new EnergyLoss
            {
                Id = (int)row["EnergyLossId"],
                TurbineId = (int)row["TurbineId"],
                // Verificar si BatteryId es DBNull antes de asignarlo // Asignar null si es DBNull 
                BatteryId = row["BatteryId"] != DBNull.Value ? (int?)row["BatteryId"] : null, 
                LostMWh = (decimal)row["LostMWh"],
                Reason = (string)row["Reason"],
                OccurredAt = (DateTime)row["OccurredAt"],
                CreatedAt = (DateTime)row["CreatedAt"]
            };

            return loss;
        }

    }
}
