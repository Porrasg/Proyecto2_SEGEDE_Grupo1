using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreApp
{
    // Lógica de negocio de "Operación y Cortes para Almacenamiento": calcula la
    // producción neta de cada turbina para un periodo (producción bruta según su
    // capacidad nominal, menos las horas perdidas por mantenimiento e inactividad),
    // la acredita a la batería de la turbina, y deja registro histórico para los
    // reportes de generación.
    //
    // Separado a propósito de EnergyManager (que sirve LocalBattery/GenerationHistory/
    // LossHistory de la página Engineer/Energy sobre tblEnergyGenerations/tblEnergyLosses):
    // este manager calcula y persiste el corte de producción neta contra tblEnergyProduction,
    // que alimenta Reportes y el botón "Calcular Producción del Periodo" de Admin/EnergyStorage.
    public class EnergyProductionManager
    {
        private const int DefaultPeriodDays = 7;

        // Ejecuta el corte de producción para todas las turbinas activas: calcula la
        // producción neta desde el último corte registrado (o los últimos 7 días si
        // nunca se ha calculado) hasta ahora, la acredita a la batería de la turbina
        // y guarda el registro histórico.
        public List<EnergyProduction> ExecutePeriodProduction()
        {
            var turbineCrud = new TurbineCrudFactory();
            var turbines = turbineCrud.RetrieveAll<Turbine>();

            var results = new List<EnergyProduction>();

            foreach (var turbine in turbines.Where(t => t.Status != "Decommissioned"))
            {
                var record = CalculateAndRecordProduction(turbine.Id);
                if (record != null)
                {
                    results.Add(record);
                }
            }

            return results;
        }

        // Calcula y registra la producción neta de una turbina específica desde su
        // último corte (o los últimos 7 días si nunca se ha calculado). Devuelve null
        // si la turbina no tiene una batería asociada (no se puede acreditar la energía).
        public EnergyProduction CalculateAndRecordProduction(int turbineId)
        {
            var turbineCrud = new TurbineCrudFactory();
            var turbine = turbineCrud.RetrieveById<Turbine>(turbineId);

            if (turbine == null)
            {
                throw new Exception("La turbina no existe");
            }

            var epCrud = new EnergyProductionCrudFactory();
            var last = epCrud.RetrieveLast(turbineId);

            var periodStart = last?.EventDate ?? DateTime.Now.AddDays(-DefaultPeriodDays);
            var periodEnd = DateTime.Now;

            if (periodEnd <= periodStart)
            {
                return null; // ya se calculó producción para este instante, nada que hacer
            }

            var (gross, maintenanceLoss, net) = CalculateNetProduction(turbine, periodStart, periodEnd);

            var record = new EnergyProduction
            {
                TurbineId = turbine.Id,
                PeriodStart = periodStart,
                EventDate = periodEnd,
                GrossEnergyMWh = gross,
                MaintenanceLossMWh = maintenanceLoss,
                GeneratedEnergy = net,
                CreatedAt = DateTime.Now
            };

            epCrud.Create(record);

            // Acreditar la producción neta a la batería de la turbina, si tiene una asignada
            if (net > 0)
            {
                var batteryCrud = new BatteriesCrudFactory();
                var battery = batteryCrud.RetrieveAll<Battery>().FirstOrDefault(b => b.TurbineId == turbine.Id && b.Status == "Active");

                if (battery != null)
                {
                    new BatteryManager().StoreEnergy(battery.Id, net);
                }
            }

            return record;
        }

        // Producción bruta (capacidad nominal semanal prorrateada a las horas del periodo)
        // menos las horas del periodo que se solapan con mantenimientos no cancelados
        // (aproximación razonable de "mantenimientos + inactividad": mientras la turbina
        // está en mantenimiento no genera). Si la turbina no está Active en este momento,
        // se considera inactiva durante todo el periodo (no hay historial de estados para
        // reconstruir cuándo exactamente dejó de estar activa).
        private (decimal gross, decimal maintenanceLoss, decimal net) CalculateNetProduction(
            Turbine turbine, DateTime periodStart, DateTime periodEnd)
        {
            var totalHours = (decimal)(periodEnd - periodStart).TotalHours;
            var hoursPerWeek = 7m * 24m;

            var gross = turbine.NominalWeeklyCapacityMWh * (totalHours / hoursPerWeek);

            if (turbine.Status != "Active")
            {
                return (Math.Round(gross, 4), Math.Round(gross, 4), 0m);
            }

            var maintenanceCrud = new MaintenanceCrudFactory();
            var maintenances = maintenanceCrud.RetrieveByTurbineId(turbine.Id);

            decimal maintenanceHours = 0;
            foreach (var m in maintenances.Where(m => m.Status != "Cancelled"))
            {
                var start = m.ActualStartDate ?? m.EstimatedStartDate;
                var end = m.ActualEndDate ?? m.EstimatedEndDate;

                var overlapStart = start > periodStart ? start : periodStart;
                var overlapEnd = end < periodEnd ? end : periodEnd;

                if (overlapEnd > overlapStart)
                {
                    maintenanceHours += (decimal)(overlapEnd - overlapStart).TotalHours;
                }
            }

            var maintenanceLoss = turbine.NominalWeeklyCapacityMWh * (maintenanceHours / hoursPerWeek);
            if (maintenanceLoss > gross) maintenanceLoss = gross;

            var net = gross - maintenanceLoss;

            return (Math.Round(gross, 4), Math.Round(maintenanceLoss, 4), Math.Round(net, 4));
        }

        public List<EnergyProduction> RetrieveGenerationHistory(int turbineId)
        {
            var crud = new EnergyProductionCrudFactory();
            return crud.RetrieveByTurbineId(turbineId);
        }

        public List<EnergyProduction> RetrieveAllProductions()
        {
            var crud = new EnergyProductionCrudFactory();

            return crud.RetrieveAllProductions();
        }
    }
}
