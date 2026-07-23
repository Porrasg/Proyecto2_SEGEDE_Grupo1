using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class TurbineManager
    {
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
                throw new Exception("El identificador de la turbina no es válido");
            }

            var tCrud = new TurbineCrudFactory();

            var turbine = tCrud.RetrieveById<Turbine>(id);

            if (turbine == null)
            {
                throw new Exception("La turbina solicitada no existe");
            }

            return turbine;
        }

        // Cambia únicamente el estado operativo de una turbina existente
        public void ChangeState(int turbineId, string newState)
        {
            if (turbineId <= 0)
            {
                throw new Exception("El identificador de la turbina no es válido");
            }

            var tCrud = new TurbineCrudFactory();

            var currentTurbine = tCrud.RetrieveById<Turbine>(turbineId);

            if (currentTurbine == null)
            {
                throw new Exception("La turbina que desea actualizar no existe");
            }

            // Se reutiliza la misma validación de estados del resto del manager
            currentTurbine.Status = newState;

            if (!IsValidStatus(currentTurbine))
            {
                throw new Exception("El estado debe ser Activa, Inactiva, En Mantenimiento, Dañada o Dada de Baja");
            }

            currentTurbine.UpdatedAt = DateTime.Now;

            tCrud.Update(currentTurbine);
        }

        public void Create(Turbine turbine)
        {
            if (turbine == null)
            {
                throw new Exception("La turbina no puede ser nula");
            }

            if (HasEmptyFields(turbine))
            {
                throw new Exception("Todos los campos obligatorios deben estar completos");
            }

            // Validar el año de fabricación
            if (!IsValidManufactureYear(turbine.ManufactureYear))
            {
                throw new Exception("El año de fabricación no es válido");
            }

            // Validar la capacidad nominal semanal
            if (turbine.NominalWeeklyCapacityMWh <= 0)
            {
                throw new Exception("La capacidad nominal debe ser mayor a 0");
            }

            if (!IsValidStatus(turbine))
            {
                throw new Exception( "El estado debe ser Activa, Inactiva, En Mantenimiento, Dañada o Dada de Baja");
            }

            var tCrud = new TurbineCrudFactory();

            var turbineByCode = tCrud.RetrieveByCode(turbine.Code);

            if (turbineByCode != null)
            {
                throw new Exception("Ya existe una turbina registrada con ese código");
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
                throw new Exception("La turbina no puede ser nula");
            }

            if (turbine.Id <= 0)
            {
                throw new Exception("El identificador de la turbina no es válido");
            }

            if (HasEmptyFields(turbine))
            {
                throw new Exception("Todos los campos obligatorios deben estar completos");
            }

            if (!IsValidManufactureYear(turbine.ManufactureYear))
            {
                throw new Exception("El año de fabricación no es válido");
            }

            if (turbine.NominalWeeklyCapacityMWh <= 0)
            {
                throw new Exception("La capacidad nominal debe ser mayor a 0");
            }

            if (!IsValidStatus(turbine))
            {
                throw new Exception("El estado debe ser Activa, Inactiva, En Mantenimiento, Dañada o Dada de Baja");
            }

            var tCrud = new TurbineCrudFactory();

            var currentTurbine = tCrud.RetrieveById<Turbine>(turbine.Id);

            if (currentTurbine == null)
            {
                throw new Exception("La turbina que desea actualizar no existe");
            }

            var turbineByCode = tCrud.RetrieveByCode(turbine.Code);

            if (turbineByCode != null && turbineByCode.Id != turbine.Id)
            {
                throw new Exception("Ya existe otra turbina registrada con ese código");
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
                throw new Exception("La turbina no puede ser nula");
            }

            if (turbine.Id <= 0)
            {
                throw new Exception("El identificador de la turbina no es válido");
            }

            var tCrud = new TurbineCrudFactory();

            var currentTurbine = tCrud.RetrieveById<Turbine>(turbine.Id);

            if (currentTurbine == null)
            {
                throw new Exception("La turbina que desea eliminar no existe");
            }

            if (currentTurbine.Status == "Decommissioned")
            {
                throw new Exception("La turbina ya se encuentra dada de baja");
            }

            // Se actualiza el estado a "Decommissioned" y se guarda la fecha de actualización
            turbine.Status = "Decommissioned";
            turbine.UpdatedAt = DateTime.Now;

            tCrud.Delete(turbine);
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
            return turbine.Status == "Active" ||
                   turbine.Status == "Inactive" ||
                   turbine.Status == "Maintenance" ||
                   turbine.Status == "Damaged" ||
                   turbine.Status == "Decommissioned";
        }
    }
}