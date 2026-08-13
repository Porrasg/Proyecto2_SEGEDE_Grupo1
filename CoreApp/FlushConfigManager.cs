using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;
namespace CoreApp
{
    public class FlushConfigManager
    {
        public FlushConfig RetrieveConfiguration()
        {
            var crud = new FlushConfigCrudFactory();

            var config = crud.RetrieveById<FlushConfig>(1);

            if (config == null)
            {
                throw new Exception(
                    "No se encontró la configuración de Flush."
                );
            }

            return config;
        }
        public void UpdateConfiguration(FlushConfig config)
        {
            if (config == null)
            {
                throw new Exception(
                    "La configuración de Flush no puede ser nula."
                );
            }

            if (config.Id <= 0)
            {
                throw new Exception(
                    "El identificador de la configuración no es válido."
                );
            }

            if (config.ExecutionTime < TimeSpan.Zero ||
                config.ExecutionTime >= TimeSpan.FromDays(1))
            {
                throw new Exception(
                    "La hora de ejecución no es válida."
                );
            }

            var crud = new FlushConfigCrudFactory();

            var currentConfig =
                crud.RetrieveById<FlushConfig>(config.Id);

            if (currentConfig == null)
            {
                throw new Exception(
                    "No se encontró la configuración de Flush."
                );
            }

            // Mantener la fecha de creación original
            config.CreatedAt = currentConfig.CreatedAt;

            // Actualizar la fecha de modificación
            config.UpdatedAt = DateTime.Now;

            crud.Update(config);
        }
    }
}
