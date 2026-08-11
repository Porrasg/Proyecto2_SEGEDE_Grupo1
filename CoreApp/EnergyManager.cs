using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class EnergyManager
    {

        public void CreateGeneration(EnergyGeneration generation) 
        {
            ValidateGenerationData(generation);

            ValidateTurbineExists(generation.TurbineId);

            ValidateBatteryExists(generation.TurbineId);

            //Fecha de creacion del registro
            generation.CreateAt = DateTime.Now;

            //Enviar la operacion al CRUD
            var generationCrud = new EnergyGenerationCrudFactory();
            generationCrud.Create(generation);
        }


        private void ValidateGenerationData(EnergyGeneration generation) 
        {
            if (generation == null)
            {
                throw new Exception("La energía no puede ser nula");
            }

            if (generation.TurbineId <= 0)
            {
                throw new Exception("El id de la turbina no es válido");
            }

            if (generation.GenerateMWh <= 0)
            {
                throw new Exception("La energía generada debe ser mayor que cero");
            }

            if (generation.WindSpeedMs < 0)
            {
                throw new Exception("La velocidad del viento no puede ser negativa");
            }

            if (generation.GenerateAt == default(DateTime))
            {
                throw new Exception("La fecha de generación es obligatoria");
            }

            if (generation.GenerateAt > DateTime.Now)
            {
                throw new Exception("La fecha de generación no puede ser mayor a la actual");
            }
        }

        private void ValidateTurbineExists(int turbineId) 
        {
            //Validar que la turbina exista
            var turbineCrud = new TurbineCrudFactory();

            // Validar que la turbina existe en la base de datos
            var turbine = turbineCrud.RetrieveById<Turbine>(turbineId);

            if (turbine == null)
            {
                throw new Exception("La turbina seleccionada no existe");
            }
            if (turbine.Status == "Decommissioned") 
            {
                throw new Exception("No se puede registrar generación para una turbina dada de baja");
            }
        }

        private void ValidateBatteryExists(int turbineId) 
        {
            //Validar que exista una bateria activa y asociada a la turbina
            var batteryCrud = new BatteriesCrudFactory();
            // Obtener todas las baterías
            var batteries = batteryCrud.RetrieveAll<Battery>();

            // Buscar la batería activa asociada a la turbina que está generando energía
            var battery = batteries.FirstOrDefault(b => b.TurbineId == turbineId && b.Status == "Active");

            if (battery == null)
            {
                throw new Exception("No existe una bateria activa asociada a la turbina seleccionada");
            }
        }




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
