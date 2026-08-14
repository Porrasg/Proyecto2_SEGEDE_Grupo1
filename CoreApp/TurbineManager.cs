using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class TurbineManager
    {
        // Punto único de configuración de los estados operativos válidos de una turbina.
        // Agregar/quitar un estado solo requiere tocar este diccionario: tanto la
        // validación del backend (IsValidStatus) como el selector de la UI (vía
        // TurbinesController.Statuses) leen de aquí, no hay listas duplicadas.
        public static readonly Dictionary<string, string> ValidStatuses = new Dictionary<string, string>
        {
            { "Active", "Activa" },
            { "Inactive", "Inactiva" },
            { "Maintenance", "En Mantenimiento" },
            { "Damaged", "Dañada" },
            { "Decommissioned", "Dada de Baja" }
        };
        public static readonly Dictionary<string, string[]> AllowedTransitions = new Dictionary<string, string[]>
        {
            { "Active", new[] { "Inactive", "Maintenance", "Damaged", "Decommissioned" } },
            { "Inactive", new[] { "Active", "Maintenance", "Damaged", "Decommissioned" } },
            { "Maintenance", new[] { "Active", "Inactive", "Damaged", "Decommissioned" } },
            { "Damaged", new[] { "Maintenance", "Decommissioned" } },
            { "Decommissioned", Array.Empty<string>() }
        };
        private static string InvalidStatusMessage =>
            "El estado debe ser " + string.Join(", ", ValidStatuses.Values);
        public List<Turbine> RetrieveAllTurbines()
        {
            var tCrud = new TurbineCrudFactory();
            return tCrud.RetrieveAll<Turbine>();
        }
        public Turbine RetrieveTurbineById(int id)
        {
            // Validar el identificador de la turbina
            if (id <= 0)
            {
                throw new BusinessException("El identificador de la turbina no es válido");
            }

            var tCrud = new TurbineCrudFactory();

            var turbine = tCrud.RetrieveById<Turbine>(id);

            if (turbine == null)
            {
                throw new BusinessException("La turbina solicitada no existe");
            }

            return turbine;
        }
        public TurbineOperationalMetrics RetrieveOperationalMetrics(int turbineId, int periodDays = 30)
        {
            RetrieveTurbineById(turbineId);
            var failures = new FailureManager().RetrieveByTurbineId(turbineId);
            var maintenances = new MaintenanceManager().RetrieveByTurbineId(turbineId);
            return TurbineMetricsCalculator.Calculate(failures, maintenances, DateTime.Now, periodDays);
        }
        // Cambia únicamente el estado operativo de una turbina existente
        public void ChangeState(int turbineId, string newState)
        {
            if (turbineId <= 0)
            {
                throw new BusinessException("El identificador de la turbina no es válido");
            }

            var tCrud = new TurbineCrudFactory();

            var currentTurbine = tCrud.RetrieveById<Turbine>(turbineId);

            if (currentTurbine == null)
            {
                throw new BusinessException("La turbina que desea actualizar no existe");
            }

            if (string.Equals(currentTurbine.Status, newState, StringComparison.Ordinal))
            {
                throw new BusinessException("La turbina ya se encuentra en el estado solicitado");
            }

            if (!AllowedTransitions.TryGetValue(currentTurbine.Status, out var allowed) ||
                !allowed.Contains(newState, StringComparer.Ordinal))
            {
                throw new BusinessException($"La transición de {currentTurbine.Status} a {newState} no está permitida");
            }
            // Se reutiliza la misma validación de estados del resto del manager
            currentTurbine.Status = newState;

            if (!IsValidStatus(currentTurbine))
            {
                throw new BusinessException(InvalidStatusMessage);
            }

            currentTurbine.UpdatedAt = DateTime.Now;

            tCrud.Update(currentTurbine);
            //actualizar en el centralBank
            var centralBankManager = new CentralBankManager();
            centralBankManager.UpdateMaximumCapacity();
        }
        public IReadOnlyList<string> GetAllowedTransitions(int turbineId)
        {
            var turbine = RetrieveTurbineById(turbineId);
            return AllowedTransitions.TryGetValue(turbine.Status, out var allowed)
                ? allowed
                : Array.Empty<string>();
        }
        public void Create(Turbine turbine)
        {
            if (turbine == null)
            {
                throw new BusinessException("La turbina no puede ser nula");
            }

            if (HasEmptyFields(turbine))
            {
                throw new BusinessException("Todos los campos obligatorios deben estar completos");
            }
            // Validar el año de fabricación
            if (!IsValidManufactureYear(turbine.ManufactureYear))
            {
                throw new BusinessException("El año de fabricación no es válido");
            }
            // Validar la capacidad nominal semanal
            if (turbine.NominalWeeklyCapacityMWh <= 0)
            {
                throw new BusinessException("La capacidad nominal debe ser mayor a 0");
            }

            if (!IsValidStatus(turbine))
            {
                throw new BusinessException(InvalidStatusMessage);
            }

            var tCrud = new TurbineCrudFactory();

            var turbineByCode = tCrud.RetrieveByCode(turbine.Code);

            if (turbineByCode != null)
            {
                throw new BusinessException("Ya existe una turbina registrada con ese código");
            }
            // Se asignan las fechas de creación y actualización
            turbine.CreatedAt = DateTime.Now;
            turbine.UpdatedAt = null;

            tCrud.Create(turbine);
        }
        public void Update(Turbine turbine)
        {
            if (turbine == null)
            {
                throw new BusinessException("La turbina no puede ser nula");
            }

            if (turbine.Id <= 0)
            {
                throw new BusinessException("El identificador de la turbina no es válido");
            }

            if (HasEmptyFields(turbine))
            {
                throw new BusinessException("Todos los campos obligatorios deben estar completos");
            }

            if (!IsValidManufactureYear(turbine.ManufactureYear))
            {
                throw new BusinessException("El año de fabricación no es válido");
            }

            if (turbine.NominalWeeklyCapacityMWh <= 0)
            {
                throw new BusinessException("La capacidad nominal debe ser mayor a 0");
            }

            if (!IsValidStatus(turbine))
            {
                throw new BusinessException(InvalidStatusMessage);
            }

            var tCrud = new TurbineCrudFactory();

            var currentTurbine = tCrud.RetrieveById<Turbine>(turbine.Id);

            if (currentTurbine == null)
            {
                throw new BusinessException("La turbina que desea actualizar no existe");
            }

            if (!string.Equals(turbine.Status, currentTurbine.Status, StringComparison.Ordinal))
            {
                throw new ArgumentException("El estado operativo no se puede cambiar desde la edición general; utilice el cambio de estado con motivo técnico");
            }

            var turbineByCode = tCrud.RetrieveByCode(turbine.Code);

            if (turbineByCode != null && turbineByCode.Id != turbine.Id)
            {
                throw new BusinessException("Ya existe otra turbina registrada con ese código");
            }
            // Se conservan las fechas de creación y se guarda la fecha de actualización
            turbine.CreatedAt = currentTurbine.CreatedAt;
            turbine.UpdatedAt = DateTime.Now;

            tCrud.Update(turbine);
        }
        public void Delete(Turbine turbine)
        {
            if (turbine == null)
            {
                throw new BusinessException("La turbina no puede ser nula");
            }

            if (turbine.Id <= 0)
            {
                throw new BusinessException("El identificador de la turbina no es válido");
            }

            var tCrud = new TurbineCrudFactory();

            var currentTurbine = tCrud.RetrieveById<Turbine>(turbine.Id);

            if (currentTurbine == null)
            {
                throw new BusinessException("La turbina que desea eliminar no existe");
            }

            if (currentTurbine.Status == "Decommissioned")
            {
                throw new BusinessException("La turbina ya se encuentra dada de baja");
            }

            if (!AllowedTransitions.TryGetValue(currentTurbine.Status, out var allowed) ||
                !allowed.Contains("Decommissioned", StringComparer.Ordinal))
            {
                throw new ArgumentException($"La transición de {currentTurbine.Status} a Decommissioned no está permitida");
            }
            // La baja es lógica y siempre utiliza el registro persistido, nunca los
            // campos que el cliente pudo enviar junto con el identificador.
            currentTurbine.Status = "Decommissioned";
            currentTurbine.UpdatedAt = DateTime.Now;

            tCrud.Delete(currentTurbine);
            new CentralBankManager().UpdateMaximumCapacity();
        }
        private bool HasEmptyFields(Turbine turbine)
        {
            return string.IsNullOrWhiteSpace(turbine.Code) ||
                   string.IsNullOrWhiteSpace(turbine.Name) ||
                   string.IsNullOrWhiteSpace(turbine.Location) ||
                   string.IsNullOrWhiteSpace(turbine.Brand) ||
                   string.IsNullOrWhiteSpace(turbine.Model) ||
                   string.IsNullOrWhiteSpace(turbine.Status);
        }
        private bool IsValidManufactureYear(int manufactureYear)
        {
            // Validar que el año de fabricación esté entre 1800 y el año actual
            return manufactureYear >= 1800 &&
                   manufactureYear <= DateTime.Now.Year;
        }

        private bool IsValidStatus(Turbine turbine)
        {
            return turbine.Status != null && ValidStatuses.ContainsKey(turbine.Status);
        }
    }
}
