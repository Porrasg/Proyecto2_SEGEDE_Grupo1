using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
        // Método para determinar la razón de la pérdida de energía según el estado de la turbina
        private string GetLossReason(string status)
        {
            switch (status)
            {
                case "Inactive":
                    return "Turbina inactiva";

                case "Maintenance":
                    return "Turbina en mantenimiento";

                case "Damaged":
                    return "Turbina dañada";

                default:
                    return "Turbina no disponible para generación";
            }
        }
        // Método para determinar la energía perdida y el costo de oportunidad asociado a una turbina detenida, y registrar la pérdida en la base de datos.
        private void RegisterEnergyLoss( Turbine turbine, decimal lostMWh, DateTime occurredAt)
        {
            var billingManager = new BillingManager();

            var activePrice = billingManager.RetrieveActivePrice();

            if (activePrice == null)
            {
                throw new Exception(
                    "No existe un precio activo por MWh para calcular el costo de oportunidad"
                );
            }

            decimal opportunityCost =
                lostMWh * activePrice.PriceCRCPerMWh;

            opportunityCost = Math.Round(opportunityCost, 4);

            var lossCrud = new EnergyLossCrudFactory();

            var loss = new EnergyLoss
            {
                TurbineId = turbine.Id,
                BatteryId = null,
                LostMWh = lostMWh,
                OpportunityCostCRC = opportunityCost,
                Reason = GetLossReason(turbine.Status),
                OccurredAt = occurredAt,
                CreatedAt = DateTime.Now
            };

            lossCrud.Create(loss);
        }
        // Método para procesar la energía generada por una turbina mientas esta activa o detenida, y registrar las pérdidas de energía si la turbina está detenida.
        public void ProcessTurbineEnergy(Turbine turbine, decimal hours)
        {
            if (turbine == null)
            {
                throw new Exception("La turbina no puede ser nula");
            }

            if (hours <= 0)
            {
                throw new Exception("La cantidad de horas debe ser mayor a cero");
            }

            // Una turbina dada de baja no participa en la simulación
            if (turbine.Status == "Decommissioned")
            {
                return;
            }

            // Calcular la producción correspondiente a las horas transcurridas.
            decimal weeklyHours = 7m * 24m;

            decimal expectedEnergy =
                turbine.NominalWeeklyCapacityMWh *
                (hours / weeklyHours);

            expectedEnergy = Math.Round(expectedEnergy, 4);

            if (expectedEnergy <= 0)
            {
                return;
            }

            // TURBINA ACTIVA
            if (turbine.Status == "Active")
            {
                var batteryCrud = new BatteriesCrudFactory();

                var battery = batteryCrud
                    .RetrieveAll<Battery>()
                    .FirstOrDefault(b =>
                        b.TurbineId == turbine.Id &&
                        b.Status == "Active");

                if (battery == null)
                {
                    // No podemos almacenar la generación si no hay batería.
                    return;
                }

                var generation = new EnergyGeneration
                {
                    TurbineId = turbine.Id,
                    GenerateMWh = expectedEnergy,
                    WindSpeedMs = 0,
                    GenerateAt = DateTime.Now,
                    CreateAt = DateTime.Now
                };

                var generationCrud = new EnergyGenerationCrudFactory();

                // El SP se encarga de: registrar generación, almacenar energía, controlar saturación, registrar pérdidas por saturación
                generationCrud.Create(generation);

                return;
            }

            // TURBINA DETENIDA
           
            RegisterEnergyLoss(turbine, expectedEnergy, DateTime.Now);
        }

    }
}
