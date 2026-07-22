using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class FlushCrudFactory : CrudFactory
    {
        public FlushCrudFactory() {
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Flush
            var flush = baseDTO as Flush;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_FLUSH_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddIntParameter("FlushBatchId", flush.FlushBatchId);
            sqlOperation.AddIntParameter("TurbineId", flush.TurbineId);
            sqlOperation.AddIntParameter("BatteryId", flush.BatteryId);
            sqlOperation.AddIntParameter("CentralBankId", flush.CentralBankId);
            sqlOperation.AddStringParameter("ExecutionType", flush.ExecutionType);
            sqlOperation.AddStringParameter("Status", flush.Status);
            sqlOperation.AddDateTimeParameter("ExecutedAt", flush.ExecutedAt);
            sqlOperation.AddDateTimeParameter("CreatedAt", flush.CreatedAt);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Flush
            var flush = baseDTO as Flush;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_FLUSH_PR";

            sqlOperation.AddIntParameter("FlushId", flush.Id);
            sqlOperation.AddStringParameter("Status", flush.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", flush.UpdatedAt);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todos los flushes que se obtengan de la consulta a la BD
            var lsflush = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_FLUSH_PR";

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Flush, luego agregarlo a la lista
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var flush = BuildFlush(row);
                    lsflush.Add((T)Convert.ChangeType(flush, typeof(T)));
                }
            }
            return lsflush;
        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_FLUSH_PR";

            
            sqlOperation.AddIntParameter("FlushId", id);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Flush y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var flush = BuildFlush(lsResults[0]);
                return (T)Convert.ChangeType(flush, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Flush
            var flush = baseDTO as Flush;

            // Definir el SP
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "UPD_FLUSH_PR";

            sqlOperation.AddIntParameter("FlushId", flush.Id);
            sqlOperation.AddIntParameter("FlushBatchId", flush.FlushBatchId);
            sqlOperation.AddIntParameter("TurbineId", flush.TurbineId);
            sqlOperation.AddIntParameter("BatteryId", flush.BatteryId);
            sqlOperation.AddIntParameter("CentralBankId", flush.CentralBankId);
            sqlOperation.AddDecimalParameter("SnapshotEnergyMWh", flush.SnapshotEnergyMWh);
            sqlOperation.AddDecimalParameter("TransferredEnergyMWh", flush.TransferredEnergyMWh);
            sqlOperation.AddDecimalParameter("SaturationLossMWh", flush.SaturationLossMWh);
            sqlOperation.AddStringParameter("ExecutionType", flush.ExecutionType);
            sqlOperation.AddStringParameter("Status", flush.Status);
            sqlOperation.AddDateTimeParameter("ExecutedAt", flush.ExecutedAt);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", flush.UpdatedAt);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        // Metodo que construye el DTO Flush a partir de la data que viene en la consulta de la BD
        private Flush BuildFlush(Dictionary<string, object> row)
        {
            var flush = new Flush
            {
                Id = (int)row["FlushId"],
                FlushBatchId = (int)row["FlushBatchId"],
                TurbineId = (int)row["TurbineId"],
                BatteryId = (int)row["BatteryId"],
                CentralBankId = (int)row["CentralBankId"],
                SnapshotEnergyMWh = (decimal)row["SnapshotEnergyMWh"],
                TransferredEnergyMWh = (decimal)row["TransferredEnergyMWh"],
                SaturationLossMWh = (decimal)row["SaturationLossMWh"],
                ExecutionType = (string)row["ExecutionType"],
                Status = (string)row["Status"],
                ExecutedAt = (DateTime)row["ExecutedAt"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? (DateTime?)row["UpdatedAt"] : null // para manejar el caso de que UpdatedAt pueda ser nulo
            };
            return flush;
        }
    }
}
