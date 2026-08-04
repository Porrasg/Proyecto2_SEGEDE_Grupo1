using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class EnergyManager
    {
        // Método para recuperar el historial de generación de energía de una turbina específica
        public List<EnergyGeneration> RetrieveGenerationHistory(int turbineId) 
        {
            // Validar el id de la turbina que existe en la base de datos
            if (turbineId <= 0)
            {
                throw new Exception("El id de la turbina no es válido");
            }

            // Validar que la turbina existe en la base de datos
            var turbineCrud = new TurbineCrudFactory();
            var turbine = turbineCrud.RetrieveById<Turbine>(turbineId);

            if (turbine == null)
            {
                throw new Exception("La turbina no existe en la base de datos");
            }

            // Validar que la turbina está activa 
            var generationCrud = new EnergyGenerationCrudFactory();

            // Retornar el historial de generación de energía de la turbina
            return generationCrud.RetrieveByTurbine(turbineId);
        }


        public List<EnergyLoss> RetrieveLossHistory(int turbineId) 
        {
            // Validar el id de la turbina que existe en la base de datos
            if (turbineId <= 0) 
            {
                throw new Exception("El id de la turbina no es válido");
            }

            // Validar que la turbina existe en la base de datos
            var turbineCrud = new TurbineCrudFactory();
            var turbine = turbineCrud.RetrieveById<Turbine>(turbineId);

            if (turbine == null) 
            {
                throw new Exception("La turbina no existe en la base de datos");
            }
            // Obtener el historial de pérdidas de energía de la turbina
            var lossCrud = new EnergyLossCrudFactory();
            
            return lossCrud.RetrieveByTurbine(turbineId);
        }


    }
}
