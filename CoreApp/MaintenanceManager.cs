using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class MaintenanceManager
    {
        public List<Maintenance> RetrieveAllMaintenances()
        {
            var crud = new MaintenanceCrudFactory();
            return crud.RetrieveAll<Maintenance>();
        }

        // Obtiene los mantenimientos de una turbina específica
        public List<Maintenance> RetrieveByTurbineId(int turbineId)
        {
            // Validar el identificador de la turbina
            if (turbineId <= 0)
            {
                throw new Exception("El identificador de la turbina no es válido");
            }

            var crud = new MaintenanceCrudFactory();

            return crud.RetrieveByTurbineId(turbineId);
        }


        public Maintenance RetrieveById(int id)
        {
            if (id <= 0)
            {
                throw new Exception("El identificador del mantenimiento no es válido");
            }

            var crud = new MaintenanceCrudFactory();

            var maintenance = crud.RetrieveById<Maintenance>(id);

            if (maintenance == null)
            {
                throw new Exception("No se encontró el mantenimiento solicitado");
            }

            return maintenance;
        }


        public void Create(Maintenance maintenance)
        {
            if (maintenance == null)
            {
                throw new Exception("El mantenimiento no puede ser nulo");
            }

            // Asignar el estado inicial del mantenimiento
            maintenance.Status = "Scheduled";

            if (HasEmptyFields(maintenance))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar el tipo de mantenimiento
            if (!IsValidMaintenanceType(maintenance))
            {
                throw new Exception("El tipo de mantenimiento no es válido");
            }

            // Validar el estado del mantenimiento
            if (!IsValidStatus(maintenance))
            {
                throw new Exception("El estado del mantenimiento no es válido");
            }

            // Validar las fechas estimadas del mantenimiento
            if (maintenance.EstimatedEndDate <= maintenance.EstimatedStartDate)
            {
                throw new Exception("La fecha estimada final debe ser posterior a la fecha estimada inicial");
            }

            // Validar que la fecha estimada no esté vacía
            if (maintenance.EstimatedStartDate == default(DateTime) ||
                maintenance.EstimatedEndDate == default(DateTime))
            {
                throw new Exception("Las fechas estimadas del mantenimiento son obligatorias");
            }

            // Validar que no se registre una fecha final sin una inicial
            if (maintenance.ActualEndDate.HasValue &&
                !maintenance.ActualStartDate.HasValue)
            {
                throw new Exception("No se puede registrar una fecha real final sin una fecha real inicial");
            }

            // Validar las fechas reales del mantenimiento
            if (maintenance.ActualStartDate.HasValue &&
                maintenance.ActualEndDate.HasValue &&
                maintenance.ActualEndDate.Value <= maintenance.ActualStartDate.Value)
            {
                throw new Exception("La fecha real final debe ser posterior a la fecha real inicial");
            }

            var turbineCrud = new TurbineCrudFactory();

            var turbine = turbineCrud.RetrieveById<Turbine>(maintenance.TurbineId);

            if (turbine == null)
            {
                throw new Exception("La turbina seleccionada no existe");
            }

            if (turbine.Status == "Decommissioned")
            {
                throw new Exception("No se puede registrar un mantenimiento para una turbina dada de baja");
            }

            var userCrud = new UserCrudFactory();

            var engineer = userCrud.RetrieveById<User>(maintenance.EngineerId);

            if (engineer == null)
            {
                throw new Exception("El ingeniero seleccionado no existe");
            }

            if (engineer.Role != "Engineer")
            {
                throw new Exception("El usuario seleccionado no tiene el rol de ingeniero");
            }

            if (engineer.Status != "Active")
            {
                throw new Exception("El ingeniero seleccionado no se encuentra activo");
            }

            var crud = new MaintenanceCrudFactory();

            var maintenances = crud.RetrieveAll<Maintenance>();

            // Validar que no haya más de un mantenimiento programado para el mismo día en diferentes turbinas
            foreach (var currentMaintenance in maintenances)
            {
                if (currentMaintenance.Status != "Cancelled" &&
                    currentMaintenance.EstimatedStartDate.Date ==
                    maintenance.EstimatedStartDate.Date &&
                    currentMaintenance.TurbineId != maintenance.TurbineId)
                {
                    throw new Exception("No se puede programar el mantenimiento de más de una turbina el mismo día");
                }
            }

            // Asignar los valores iniciales del mantenimiento
            maintenance.ActualStartDate = null;
            maintenance.ActualEndDate = null;
            maintenance.Result = null;
            maintenance.CreatedAt = DateTime.Now;
            maintenance.UpdatedAt = null;

            crud.Create(maintenance);
        }


        public void Update(Maintenance maintenance)
        {
            if (maintenance == null)
            {
                throw new Exception("El mantenimiento no puede ser nulo");
            }

            if (maintenance.Id <= 0)
            {
                throw new Exception("El identificador del mantenimiento no es válido");
            }

            if (HasEmptyFields(maintenance))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar el tipo de mantenimiento
            if (!IsValidMaintenanceType(maintenance))
            {
                throw new Exception("El tipo de mantenimiento no es válido");
            }

            // Validar el estado del mantenimiento
            if (!IsValidStatus(maintenance))
            {
                throw new Exception("El estado del mantenimiento no es válido");
            }

            // Validar que las fechas estimadas sean obligatorias
            if (maintenance.EstimatedStartDate == default(DateTime) ||
                maintenance.EstimatedEndDate == default(DateTime))
            {
                throw new Exception("Las fechas estimadas del mantenimiento son obligatorias");
            }

            // Validar las fechas estimadas del mantenimiento
            if (maintenance.EstimatedEndDate <= maintenance.EstimatedStartDate)
            {
                throw new Exception("La fecha estimada final debe ser posterior a la fecha estimada inicial");
            }

            // Validar que no se registre una fecha real final sin una inicial
            if (maintenance.ActualEndDate.HasValue &&
                !maintenance.ActualStartDate.HasValue)
            {
                throw new Exception("No se puede registrar una fecha real final sin una fecha real inicial");
            }

            // Validar las fechas reales del mantenimiento
            if (maintenance.ActualStartDate.HasValue &&
                maintenance.ActualEndDate.HasValue &&
                maintenance.ActualEndDate.Value <= maintenance.ActualStartDate.Value)
            {
                throw new Exception("La fecha real final debe ser posterior a la fecha real inicial");
            }

            // Validar la información de un mantenimiento completado
            if (maintenance.Status == "Completed" &&
                (!maintenance.ActualStartDate.HasValue ||
                 !maintenance.ActualEndDate.HasValue ||
                 string.IsNullOrWhiteSpace(maintenance.Result)))
            {
                throw new Exception("Un mantenimiento completado debe tener fechas reales y un resultado");
            }

            var crud = new MaintenanceCrudFactory();

            var currentMaintenance =
                crud.RetrieveById<Maintenance>(maintenance.Id);

            if (currentMaintenance == null)
            {
                throw new Exception("El mantenimiento que desea actualizar no existe");
            }

            // Validar que el mantenimiento no esté cancelado
            if (currentMaintenance.Status == "Cancelled")
            {
                throw new Exception("No se puede modificar un mantenimiento cancelado");
            }

            var turbineCrud = new TurbineCrudFactory();

            var turbine = turbineCrud.RetrieveById<Turbine>(
                maintenance.TurbineId);

            if (turbine == null)
            {
                throw new Exception("La turbina seleccionada no existe");
            }

            // Validar que la turbina no esté dada de baja
            if (turbine.Status == "Decommissioned")
            {
                throw new Exception("No se puede asignar el mantenimiento a una turbina dada de baja");
            }

            var userCrud = new UserCrudFactory();

            var engineer = userCrud.RetrieveById<User>(
                maintenance.EngineerId);

            if (engineer == null)
            {
                throw new Exception("El ingeniero seleccionado no existe");
            }

            if (engineer.Role != "Engineer")
            {
                throw new Exception("El usuario seleccionado no tiene el rol de ingeniero");
            }

            if (engineer.Status != "Active")
            {
                throw new Exception("El ingeniero seleccionado no se encuentra activo");
            }

            var maintenances = crud.RetrieveAll<Maintenance>();

            // Validar que no exista un mantenimiento para otra turbina el mismo día
            foreach (var current in maintenances)
            {
                if (current.Id != maintenance.Id &&
                    current.Status != "Cancelled" &&
                    current.EstimatedStartDate.Date ==
                    maintenance.EstimatedStartDate.Date &&
                    current.TurbineId != maintenance.TurbineId)
                {
                    throw new Exception("No se puede programar el mantenimiento de más de una turbina el mismo día");
                }
            }

            // Mantener la fecha original de creación
            maintenance.CreatedAt = currentMaintenance.CreatedAt;
            // Asignar la fecha de actualización
            maintenance.UpdatedAt = DateTime.Now;

            crud.Update(maintenance);
        }

        public void Delete(Maintenance maintenance)
        {
            if (maintenance == null)
            {
                throw new Exception(
                    "El mantenimiento no puede ser nulo");
            }

            if (maintenance.Id <= 0)
            {
                throw new Exception(
                    "El identificador del mantenimiento no es válido");
            }

            var crud = new MaintenanceCrudFactory();

            var currentMaintenance =
                crud.RetrieveById<Maintenance>(maintenance.Id);

            if (currentMaintenance == null)
            {
                throw new Exception(
                    "El mantenimiento que desea eliminar no existe");
            }

            // Validar que el mantenimiento no esté cancelado
            if (currentMaintenance.Status == "Cancelled")
            {
                throw new Exception(
                    "El mantenimiento ya se encuentra cancelado");
            }

            // Validar que el mantenimiento no esté completado
            if (currentMaintenance.Status == "Completed")
            {
                throw new Exception(
                    "No se puede cancelar un mantenimiento completado");
            }

            // Asignar el estado de cancelación
            maintenance.Status = "Cancelled";
            // Asignar la fecha de actualización
            maintenance.UpdatedAt = DateTime.Now;
            
            crud.Delete(maintenance);
        }


        private bool HasEmptyFields(Maintenance m)
        {
            return m.TurbineId <= 0 ||
                   m.EngineerId <= 0 ||
                   string.IsNullOrWhiteSpace(m.MaintenanceType) ||
                   string.IsNullOrWhiteSpace(m.Description) ||
                   string.IsNullOrWhiteSpace(m.Status);
        }


        private bool IsValidMaintenanceType(Maintenance maintenance)
        {
            return maintenance.MaintenanceType == "Preventive" ||
                   maintenance.MaintenanceType == "Corrective" ||
                   maintenance.MaintenanceType == "Predictive" ||
                   maintenance.MaintenanceType == "Inspection";
        }


        private bool IsValidStatus(Maintenance maintenance)
        {
            return maintenance.Status == "Scheduled" ||
                   maintenance.Status == "InProgress" ||
                   maintenance.Status == "Completed" ||
                   maintenance.Status == "Cancelled";
        }
    }
}