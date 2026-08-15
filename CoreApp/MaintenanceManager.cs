using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreApp
{
    public class MaintenanceManager
    {
        private static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            { "Scheduled", new[] { "InProgress", "Completed", "Cancelled" } },
            { "InProgress", new[] { "Completed", "Cancelled" } },
            { "Completed", Array.Empty<string>() },
            { "Cancelled", Array.Empty<string>() }
        };

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
                throw new BusinessException("El identificador de la turbina no es válido");
            }

            var crud = new MaintenanceCrudFactory();

            return crud.RetrieveByTurbineId(turbineId);
        }


        public Maintenance RetrieveById(int id)
        {
            if (id <= 0)
            {
                throw new BusinessException("El identificador del mantenimiento no es válido");
            }

            var crud = new MaintenanceCrudFactory();

            var maintenance = crud.RetrieveById<Maintenance>(id);

            if (maintenance == null)
            {
                throw new BusinessException("No se encontró el mantenimiento solicitado");
            }

            return maintenance;
        }


        public void Create(Maintenance maintenance)
        {
            if (maintenance == null)
            {
                throw new BusinessException("El mantenimiento no puede ser nulo");
            }

            // Asignar el estado inicial del mantenimiento
            maintenance.Status = "Scheduled";

            if (HasEmptyFields(maintenance))
            {
                throw new BusinessException("Todos los campos obligatorios deben completarse");
            }

            // Validar el tipo de mantenimiento
            if (!IsValidMaintenanceType(maintenance))
            {
                throw new BusinessException("El tipo de mantenimiento no es válido");
            }

            // Validar el estado del mantenimiento
            if (!IsValidStatus(maintenance))
            {
                throw new BusinessException("El estado del mantenimiento no es válido");
            }

            // Validar las fechas estimadas del mantenimiento
            if (maintenance.EstimatedEndDate <= maintenance.EstimatedStartDate)
            {
                throw new BusinessException("La fecha estimada final debe ser posterior a la fecha estimada inicial");
            }

            // Validar que la fecha estimada no esté vacía
            if (maintenance.EstimatedStartDate == default(DateTime) ||
                maintenance.EstimatedEndDate == default(DateTime))
            {
                throw new BusinessException("Las fechas estimadas del mantenimiento son obligatorias");
            }

            // Validar que no se registre una fecha final sin una inicial
            if (maintenance.ActualEndDate.HasValue &&
                !maintenance.ActualStartDate.HasValue)
            {
                throw new BusinessException("No se puede registrar una fecha real final sin una fecha real inicial");
            }

            // Validar las fechas reales del mantenimiento
            if (maintenance.ActualStartDate.HasValue &&
                maintenance.ActualEndDate.HasValue &&
                maintenance.ActualEndDate.Value <= maintenance.ActualStartDate.Value)
            {
                throw new BusinessException("La fecha real final debe ser posterior a la fecha real inicial");
            }

            var turbineCrud = new TurbineCrudFactory();

            var turbine = turbineCrud.RetrieveById<Turbine>(maintenance.TurbineId);

            if (turbine == null)
            {
                throw new BusinessException("La turbina seleccionada no existe");
            }

            if (turbine.Status == "Decommissioned")
            {
                throw new BusinessException("No se puede registrar un mantenimiento para una turbina dada de baja");
            }

            var userCrud = new UserCrudFactory();

            var engineer = userCrud.RetrieveById<User>(maintenance.EngineerId);

            if (engineer == null)
            {
                throw new BusinessException("El ingeniero seleccionado no existe");
            }

            if (engineer.Role != "Engineer")
            {
                throw new BusinessException("El usuario seleccionado no tiene el rol de ingeniero");
            }

            if (engineer.Status != "Active")
            {
                throw new BusinessException("El ingeniero seleccionado no se encuentra activo");
            }

            var crud = new MaintenanceCrudFactory();

            var maintenances = crud.RetrieveAll<Maintenance>();

            // Validar que no haya más de un mantenimiento programado para el mismo día en diferentes
            // turbinas. Un mantenimiento de tipo Emergency (falla no planificada) está exento de esta
            // regla en ambos sentidos: puede agendarse aunque otra turbina ya tenga mantenimiento ese
            // día, y no bloquea a un mantenimiento planificado que se agende después.
            if (maintenance.MaintenanceType != "Emergency")
            {
                foreach (var currentMaintenance in maintenances)
                {
                    if (currentMaintenance.Status != "Cancelled" &&
                        currentMaintenance.MaintenanceType != "Emergency" &&
                        currentMaintenance.EstimatedStartDate.Date <= maintenance.EstimatedEndDate.Date &&
                        currentMaintenance.EstimatedEndDate.Date >= maintenance.EstimatedStartDate.Date &&
                        currentMaintenance.TurbineId != maintenance.TurbineId)
                    {
                        throw new BusinessException("No se puede programar el mantenimiento de más de una turbina el mismo día");
                    }
                }
            }

            // Asignar los valores iniciales del mantenimiento
            maintenance.ActualStartDate = null;
            maintenance.ActualEndDate = null;
            maintenance.Result = null;
            maintenance.CreatedAt = DateTime.Now;
            maintenance.UpdatedAt = null;

            crud.Create(maintenance);

            // Notificar al ingeniero asignado el agendamiento del mantenimiento (correo + registro
            // en la app). Cada aviso va en su propio try/catch: un fallo nunca debe revertir el
            // mantenimiento ya creado, ni un canal debe bloquear al otro. CrudFactory.Create no
            // devuelve el Id generado por el SP, así que la notificación in-app no queda enlazada
            // a un ReferenceId específico (se deja sin referencia en vez de usar un Id inválido).
            try
            {
                var notificationManager = new NotificationManager();
                notificationManager.SendServiceScheduledNotification(
                    engineer.Email,
                    engineer.FirstName,
                    $"Turbina #{maintenance.TurbineId}",
                    $"{maintenance.EstimatedStartDate:dd/MM/yyyy HH:mm} - {maintenance.EstimatedEndDate:dd/MM/yyyy HH:mm}"
                );
            }
            catch { /* no bloquear el mantenimiento ya creado */ }

            try
            {
                new NotificationManager().Create(new Notification
                {
                    UserId = engineer.Id,
                    Title = "Mantenimiento agendado",
                    Message = $"Turbina #{maintenance.TurbineId}, del {maintenance.EstimatedStartDate:dd/MM/yyyy} al {maintenance.EstimatedEndDate:dd/MM/yyyy}.",
                    NotificationType = "Maintenance"
                });
            }
            catch { /* no bloquear el mantenimiento ya creado */ }
        }


        public void Update(Maintenance maintenance)
        {
            if (maintenance == null)
            {
                throw new BusinessException("El mantenimiento no puede ser nulo");
            }

            if (maintenance.Id <= 0)
            {
                throw new BusinessException("El identificador del mantenimiento no es válido");
            }

            if (HasEmptyFields(maintenance))
            {
                throw new BusinessException("Todos los campos obligatorios deben completarse");
            }

            // Validar el tipo de mantenimiento
            if (!IsValidMaintenanceType(maintenance))
            {
                throw new BusinessException("El tipo de mantenimiento no es válido");
            }

            // Validar el estado del mantenimiento
            if (!IsValidStatus(maintenance))
            {
                throw new BusinessException("El estado del mantenimiento no es válido");
            }

            // Validar que las fechas estimadas sean obligatorias
            if (maintenance.EstimatedStartDate == default(DateTime) ||
                maintenance.EstimatedEndDate == default(DateTime))
            {
                throw new BusinessException("Las fechas estimadas del mantenimiento son obligatorias");
            }

            // Validar las fechas estimadas del mantenimiento
            if (maintenance.EstimatedEndDate <= maintenance.EstimatedStartDate)
            {
                throw new BusinessException("La fecha estimada final debe ser posterior a la fecha estimada inicial");
            }

            // Validar que no se registre una fecha real final sin una inicial
            if (maintenance.ActualEndDate.HasValue &&
                !maintenance.ActualStartDate.HasValue)
            {
                throw new BusinessException("No se puede registrar una fecha real final sin una fecha real inicial");
            }

            // Validar las fechas reales del mantenimiento
            if (maintenance.ActualStartDate.HasValue &&
                maintenance.ActualEndDate.HasValue &&
                maintenance.ActualEndDate.Value <= maintenance.ActualStartDate.Value)
            {
                throw new BusinessException("La fecha real final debe ser posterior a la fecha real inicial");
            }

            // Validar la información de un mantenimiento completado
            if (maintenance.Status == "Completed" &&
                (!maintenance.ActualStartDate.HasValue ||
                 !maintenance.ActualEndDate.HasValue ||
                 string.IsNullOrWhiteSpace(maintenance.Result)))
            {
                throw new BusinessException("Un mantenimiento completado debe tener fechas reales y un resultado");
            }

            var crud = new MaintenanceCrudFactory();

            var currentMaintenance =
                crud.RetrieveById<Maintenance>(maintenance.Id);

            if (currentMaintenance == null)
            {
                throw new BusinessException("El mantenimiento que desea actualizar no existe");
            }

            // Los estados terminales son inmutables: no se puede reabrir ni alterar
            // un mantenimiento ya completado o cancelado.
            if (currentMaintenance.Status == "Cancelled" || currentMaintenance.Status == "Completed")
            {
                throw new BusinessException("No se puede modificar un mantenimiento completado o cancelado");
            }

            if (maintenance.TurbineId != currentMaintenance.TurbineId ||
                maintenance.EngineerId != currentMaintenance.EngineerId)
            {
                throw new BusinessException("La turbina y el ingeniero asignados al mantenimiento no se pueden modificar");
            }

            if (!string.Equals(maintenance.Status, currentMaintenance.Status, StringComparison.Ordinal) &&
                (!AllowedTransitions.TryGetValue(currentMaintenance.Status, out var allowed) ||
                 !allowed.Contains(maintenance.Status, StringComparer.Ordinal)))
            {
                throw new BusinessException($"La transición de {currentMaintenance.Status} a {maintenance.Status} no está permitida");
            }

            var turbineCrud = new TurbineCrudFactory();

            var turbine = turbineCrud.RetrieveById<Turbine>(
                maintenance.TurbineId);

            if (turbine == null)
            {
                throw new BusinessException("La turbina seleccionada no existe");
            }

            // Validar que la turbina no esté dada de baja
            if (turbine.Status == "Decommissioned")
            {
                throw new BusinessException("No se puede asignar el mantenimiento a una turbina dada de baja");
            }

            var userCrud = new UserCrudFactory();

            var engineer = userCrud.RetrieveById<User>(
                maintenance.EngineerId);

            if (engineer == null)
            {
                throw new BusinessException("El ingeniero seleccionado no existe");
            }

            if (engineer.Role != "Engineer")
            {
                throw new BusinessException("El usuario seleccionado no tiene el rol de ingeniero");
            }

            if (engineer.Status != "Active")
            {
                throw new BusinessException("El ingeniero seleccionado no se encuentra activo");
            }

            var maintenances = crud.RetrieveAll<Maintenance>();

            // Validar que no exista un mantenimiento para otra turbina el mismo día
            // (los de tipo Emergency están exentos, ver Create para la justificación)
            if (maintenance.MaintenanceType != "Emergency")
            {
                foreach (var current in maintenances)
                {
                    if (current.Id != maintenance.Id &&
                        current.Status != "Cancelled" &&
                        current.MaintenanceType != "Emergency" &&
                        current.EstimatedStartDate.Date <= maintenance.EstimatedEndDate.Date &&
                        current.EstimatedEndDate.Date >= maintenance.EstimatedStartDate.Date &&
                        current.TurbineId != maintenance.TurbineId)
                    {
                        throw new BusinessException("No se puede programar el mantenimiento de más de una turbina el mismo día");
                    }
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
                throw new BusinessException(
                    "El mantenimiento no puede ser nulo");
            }

            if (maintenance.Id <= 0)
            {
                throw new BusinessException(
                    "El identificador del mantenimiento no es válido");
            }

            var crud = new MaintenanceCrudFactory();

            var currentMaintenance =
                crud.RetrieveById<Maintenance>(maintenance.Id);

            if (currentMaintenance == null)
            {
                throw new BusinessException(
                    "El mantenimiento que desea eliminar no existe");
            }

            // Validar que el mantenimiento no esté cancelado
            if (currentMaintenance.Status == "Cancelled")
            {
                throw new BusinessException(
                    "El mantenimiento ya se encuentra cancelado");
            }

            // Validar que el mantenimiento no esté completado
            if (currentMaintenance.Status == "Completed")
            {
                throw new BusinessException(
                    "No se puede cancelar un mantenimiento completado");
            }

            // La cancelación lógica siempre usa el registro persistido.
            currentMaintenance.Status = "Cancelled";
            // Asignar la fecha de actualización
            currentMaintenance.UpdatedAt = DateTime.Now;
            
            crud.Delete(currentMaintenance);
        }


        // Turbinas que aún NO tienen ningún mantenimiento (planificado o completado, no cancelado)
        // agendado para el mes en curso. Indicador de cumplimiento de la obligatoriedad mensual
        // pedida por la rúbrica - se implementa como reporte/alerta, no como validación bloqueante,
        // porque es una obligación retrospectiva del mes (no tiene sentido impedir crear un
        // mantenimiento futuro por una obligación que se evalúa sobre el mes actual).
        public List<Turbine> RetrieveTurbinesWithoutMonthlyMaintenance()
        {
            var turbineCrud = new TurbineCrudFactory();
            var turbines = turbineCrud.RetrieveAll<Turbine>();

            var crud = new MaintenanceCrudFactory();
            var maintenances = crud.RetrieveAll<Maintenance>();

            var now = DateTime.Now;

            var turbineIdsWithMaintenanceThisMonth = new HashSet<int>();
            foreach (var m in maintenances)
            {
                if (m.Status != "Cancelled" &&
                    m.EstimatedStartDate.Month == now.Month &&
                    m.EstimatedStartDate.Year == now.Year)
                {
                    turbineIdsWithMaintenanceThisMonth.Add(m.TurbineId);
                }
            }

            var result = new List<Turbine>();
            foreach (var turbine in turbines)
            {
                // Una turbina dada de baja ya no requiere mantenimiento mensual
                if (turbine.Status == "Decommissioned")
                {
                    continue;
                }

                if (!turbineIdsWithMaintenanceThisMonth.Contains(turbine.Id))
                {
                    result.Add(turbine);
                }
            }

            return result;
        }

        public List<Turbine> RetrieveTurbinesWithOverdueMaintenance(int overdueDays = 40)
        {
            if (overdueDays <= 0)
            {
                throw new BusinessException("La cantidad de días debe ser mayor a cero");
            }

            var turbines = new TurbineCrudFactory().RetrieveAll<Turbine>();
            var maintenances = new MaintenanceCrudFactory().RetrieveAll<Maintenance>();
            var threshold = DateTime.Now.AddDays(-overdueDays);

            return turbines
                .Where(t => t.Status != "Decommissioned")
                .Where(t =>
                {
                    var lastCompleted = maintenances
                        .Where(m => m.TurbineId == t.Id && m.Status == "Completed")
                        .Select(m => m.ActualEndDate ?? m.EstimatedEndDate)
                        .OrderByDescending(date => date)
                        .FirstOrDefault();

                    return lastCompleted == default || lastCompleted < threshold;
                })
                .ToList();
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
                   maintenance.MaintenanceType == "Inspection" ||
                   maintenance.MaintenanceType == "Emergency"; // mantenimiento no planificado por falla de emergencia
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
