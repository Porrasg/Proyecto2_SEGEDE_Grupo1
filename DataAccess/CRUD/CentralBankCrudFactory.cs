using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class CentralBankCrudFactory : CrudFactory
    {
        public CentralBankCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto CentralBank
            var centralBank = baseDTO as CentralBank;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_CENTRAL_BANK_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddStringParameter("Name", centralBank.Name);
            sqlOperation.AddDecimalParameter("MaximumCapacityMWh", centralBank.MaximumCapacityMWh);
            sqlOperation.AddDecimalParameter("CurrentInventoryMWh", centralBank.CurrentInventoryMWh);
            sqlOperation.AddDecimalParameter("TotalReceivedMWh", centralBank.TotalReceivedMWh);
            sqlOperation.AddDecimalParameter("TotalDistributedMWh", centralBank.TotalDistributedMWh);
            sqlOperation.AddDecimalParameter("TotalSaturationLossMWh", centralBank.TotalSaturationLossMWh);
            sqlOperation.AddStringParameter("Status", centralBank.Status);
            sqlOperation.AddDateTimeParameter("CreatedAt", centralBank.CreatedAt);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", centralBank.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto CentralBank
            var centralBank = baseDTO as CentralBank;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_CENTRAL_BANK_PR";

            sqlOperation.AddIntParameter("CentralBankId", centralBank.Id);
            sqlOperation.AddStringParameter("Status", centralBank.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", centralBank.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todos los bancos centrales que se obtengan de la consulta a la BD
            var lsCentralBanks = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_CENTRAL_BANK_PR";

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto CentralBank, luego agregarlo a la lista de bancos centrales
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var centralBank = BuildCentralBank(row);
                    lsCentralBanks.Add((T)Convert.ChangeType(centralBank, typeof(T)));
                }
            }

            return lsCentralBanks;
        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_CENTRAL_BANK_PR";

            sqlOperation.AddIntParameter("CentralBankId", id);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto CentralBank y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var item = lsResults[0];
                var centralBank = BuildCentralBank(lsResults[0]);
                return (T)Convert.ChangeType(centralBank, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto CentralBank
            var centralBank = baseDTO as CentralBank;

            // Definir el SP
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "UPD_CENTRAL_BANK_PR";

            sqlOperation.AddIntParameter("CentralBankId", centralBank.Id);
            sqlOperation.AddStringParameter("Name", centralBank.Name);
            sqlOperation.AddDecimalParameter("MaximumCapacityMWh", centralBank.MaximumCapacityMWh);
            sqlOperation.AddDecimalParameter("CurrentInventoryMWh", centralBank.CurrentInventoryMWh);
            sqlOperation.AddDecimalParameter("TotalReceivedMWh", centralBank.TotalReceivedMWh);
            sqlOperation.AddDecimalParameter("TotalDistributedMWh", centralBank.TotalDistributedMWh);
            sqlOperation.AddDecimalParameter("TotalSaturationLossMWh", centralBank.TotalSaturationLossMWh);
            sqlOperation.AddStringParameter("Status", centralBank.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", centralBank.UpdatedAt); // puede ser nulo

            //Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        //Metodo que construye el DTO del CentralBank a partir de la data que viene en la consulta de la BD
        private CentralBank BuildCentralBank(Dictionary<string, object> row)
        {
            var centralBank = new CentralBank
            {
                Id = (int)row["CentralBankId"],
                Name = (string)row["Name"],
                MaximumCapacityMWh = (decimal)row["MaximumCapacityMWh"],
                CurrentInventoryMWh = (decimal)row["CurrentInventoryMWh"],
                TotalReceivedMWh = (decimal)row["TotalReceivedMWh"],
                TotalDistributedMWh = (decimal)row["TotalDistributedMWh"],
                TotalSaturationLossMWh = (decimal)row["TotalSaturationLossMWh"],
                Status = (string)row["Status"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? (DateTime?)row["UpdatedAt"] : null
            };
            return centralBank;
        }
    }
}
