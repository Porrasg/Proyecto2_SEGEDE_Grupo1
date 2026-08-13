using System;
using System.Collections.Generic;
using System.Text;
using Entities_DTOs;
using DataAccess.DAO;

namespace DataAccess.CRUD
{
    public class FlushConfigCrudFactory : CrudFactory
    {
        public FlushConfigCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }
        public override void Create(BaseDTO baseDTO)
        {
            throw new NotImplementedException(
                "La configuración de Flush no se crea desde el CRUD."
            );
        }
        public override void Delete(BaseDTO baseDTO)
        {
            throw new NotImplementedException(
                "La configuración de Flush no se elimina."
            );
        }
        public override List<T> RetrieveAll<T>()
        {
            var lsFlushConfig = new List<T>();

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_FLUSH_CONFIG_PR";

            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var config = BuildFlushConfig(row);

                    lsFlushConfig.Add(
                        (T)Convert.ChangeType(
                            config,
                            typeof(T)
                        )
                    );
                }
            }

            return lsFlushConfig;
        }
        public override T RetrieveById<T>(int id)
        {
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_FLUSH_CONFIG_PR";

            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lsResults.Count > 0)
            {
                var config = BuildFlushConfig(lsResults[0]);

                return (T)Convert.ChangeType(
                    config,
                    typeof(T)
                );
            }

            return default(T);
        }
        public override void Update(BaseDTO baseDTO)
        {
            var config = baseDTO as FlushConfig;

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_FLUSH_CONFIG_PR";

            sqlOperation.AddIntParameter(
                "FlushConfigId",
                config.Id
            );

            sqlOperation.AddTimeSpanParameter(
                "ExecutionTime",
                config.ExecutionTime
            );

            sqlOperation.AddBoolParameter(
                "IsAutomatic",
                config.IsAutomatic
            );

            sqlOperation.AddDateTimeParameter(
                "UpdatedAt",
                config.UpdatedAt ?? DateTime.Now
            );

            sqlDao.ExecuteProcedure(sqlOperation);
        }
        private FlushConfig BuildFlushConfig(
            Dictionary<string, object> row)
        {
            var config = new FlushConfig
            {
                Id = Convert.ToInt32(
                    row["FlushConfigId"]
                ),

                ExecutionTime = (TimeSpan)
                    row["ExecutionTime"],

                IsAutomatic = Convert.ToBoolean(
                    row["IsAutomatic"]
                ),

                CreatedAt = Convert.ToDateTime(
                    row["CreatedAt"]
                ),

                UpdatedAt =
                    row["UpdatedAt"] != DBNull.Value
                        ? Convert.ToDateTime(
                            row["UpdatedAt"]
                        )
                        : (DateTime?)null
            };

            return config;
        }
    }
}