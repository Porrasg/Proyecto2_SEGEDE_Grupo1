using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class BatteryCrudFactory : CrudFactory
    {
        public BatteryCrudFactory(){
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Battery
            var battery = baseDTO as Battery;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_BATTERY_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddIntParameter("TurbineId", battery.TurbineId);
            sqlOperation.AddDecimalParameter("MaximumCapacityMWh", battery.MaximumCapacityMWh);
            sqlOperation.AddDecimalParameter("CurrentEnergyMWh", battery.CurrentEnergyMWh);
            sqlOperation.AddDecimalParameter("TotalGeneratedMWh", battery.TotalGeneratedMWh);
            sqlOperation.AddDecimalParameter("TotalTransferredMWh", battery.TotalTransferredMWh);
            sqlOperation.AddDecimalParameter("TotalSaturationLossMWh", battery.TotalSaturationLossMWh);
            sqlOperation.AddStringParameter("Status", battery.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", battery.CreatedAt);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", battery.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Battery
            var battery = baseDTO as Battery;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_BATTERY_PR";

            sqlOperation.AddIntParameter("BatteryId", battery.Id);
            sqlOperation.AddStringParameter("Status", battery.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", battery.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todas las baterias que se obtengan de la consulta a la BD
            var lsBatteries = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_BATTERY_PR";

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Battery, luego agregarlo a la lista de baterias
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var battery = BuildBattery(row);
                    lsBatteries.Add((T)Convert.ChangeType(battery, typeof(T)));
                }
            }

            return lsBatteries;
        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_BATTERY_PR";

            sqlOperation.AddIntParameter("BatteryId", id);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Battery y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var item = lsResults[0];
                var battery = BuildBattery(lsResults[0]);
                return (T)Convert.ChangeType(battery, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Battery
            var battery = baseDTO as Battery;

            // Definir el SP
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "UPD_BATTERY_PR";

            sqlOperation.AddIntParameter("BatteryId", battery.Id);
            sqlOperation.AddIntParameter("TurbineId", battery.TurbineId);
            sqlOperation.AddDecimalParameter("MaximumCapacityMWh", battery.MaximumCapacityMWh);
            sqlOperation.AddDecimalParameter("CurrentEnergyMWh", battery.CurrentEnergyMWh);
            sqlOperation.AddDecimalParameter("TotalGeneratedMWh", battery.TotalGeneratedMWh);
            sqlOperation.AddDecimalParameter("TotalTransferredMWh", battery.TotalTransferredMWh);
            sqlOperation.AddDecimalParameter("TotalSaturationLossMWh", battery.TotalSaturationLossMWh);
            sqlOperation.AddStringParameter("Status", battery.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", battery.UpdatedAt); // puede ser nulo

            //Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        //Metodo que construye el DTO del Battery a partir de la data que viene en la consulta de la BD
        private Battery BuildBattery(Dictionary<string, object> row)
        {
            var battery = new Battery
            {
                Id = (int)row["BatteryId"],
                TurbineId = (int)row["TurbineId"],
                MaximumCapacityMWh = (decimal)row["MaximumCapacityMWh"],
                CurrentEnergyMWh = (decimal)row["CurrentEnergyMWh"],
                TotalGeneratedMWh = (decimal)row["TotalGeneratedMWh"],
                TotalTransferredMWh = (decimal)row["TotalTransferredMWh"],
                TotalSaturationLossMWh = (decimal)row["TotalSaturationLossMWh"],
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? (DateTime)row["UpdatedAt"] : (DateTime?)null
            };
            return battery;
        }
    }
}
