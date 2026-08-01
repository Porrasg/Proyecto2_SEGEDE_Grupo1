using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class BatteryManager
    {
        /*public List<Battery> RetrieveAllBatteries()
        {
            var batteryCrud = new BatteriesCrudFactory();
            return batteryCrud.RetrieveAll<Battery>();
        } */

        public List<Battery> RetrieveAllBatteries()
        {
            var batteryCrud = new BatteriesCrudFactory();

            // Forzamos la obtención explícita mapeada a través de la firma del factory concreto
            List<Battery> list = batteryCrud.RetrieveAll<Battery>();

            // Verificación de respaldo para depuración en caliente (Hot Debug)
            if (list == null)
            {
                list = new List<Battery>();
            }

            return list;
        }



        public Battery RetrieveById(int id)
        {
            if (id <= 0)
            {
                throw new Exception("El identificador de la batería no es válido");
            }

            var batteryCrud = new BatteriesCrudFactory();

            // Obtener la batería registrada
            var battery = batteryCrud.RetrieveById<Battery>(id);

            if (battery == null)
            {
                throw new Exception("No se encontró la batería solicitada");
            }

            return battery;
        }


        public void Create(Battery battery)
        {
            if (battery == null)
            {
                throw new Exception("La batería no puede ser nula");
            }

            // Asignar el estado inicial de la batería
            battery.Status = "Active";

            if (HasEmptyFields(battery))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar que la capacidad máxima sea mayor a cero
            if (battery.MaximumCapacityMWh <= 0)
            {
                throw new Exception("La capacidad máxima debe ser mayor a cero");
            }

            var turbineCrud = new TurbineCrudFactory();

            // Obtener la turbina asociada a la batería
            var turbine = turbineCrud.RetrieveById<Turbine>(battery.TurbineId);

            if (turbine == null)
            {
                throw new Exception("La turbina seleccionada no existe");
            }

            // Validar que la turbina no esté dada de baja
            if (turbine.Status == "Decommissioned")
            {
                throw new Exception("No se puede asignar una batería a una turbina dada de baja");
            }

            var batteryCrud = new BatteriesCrudFactory();

            // Obtener las baterías registradas
            var batteries = batteryCrud.RetrieveAll<Battery>();

            // Validar que la turbina no tenga una batería asignada
            foreach (var currentBattery in batteries)
            {
                if (currentBattery.TurbineId == battery.TurbineId &&
                    currentBattery.Status != "Inactive")
                {
                    throw new Exception("La turbina seleccionada ya tiene una batería asignada");
                }
            }

            // Asignar los valores iniciales de la batería
            battery.CurrentEnergyMWh = 0;
            battery.TotalGeneratedMWh = 0;
            battery.TotalTransferredMWh = 0;
            battery.TotalSaturationLossMWh = 0;
            battery.CreatedAt = DateTime.Now;
            battery.UpdatedAt = null;

            batteryCrud.Create(battery);
        }


        public void Update(Battery battery)
        {
            if (battery == null)
            {
                throw new Exception("La batería no puede ser nula");
            }

            if (battery.Id <= 0)
            {
                throw new Exception("El identificador de la batería no es válido");
            }

            if (HasEmptyFields(battery))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar la capacidad máxima
            if (battery.MaximumCapacityMWh <= 0)
            {
                throw new Exception("La capacidad máxima debe ser mayor a cero");
            }

            // Validar los valores de energía
            if (HasNegativeValues(battery))
            {
                throw new Exception("Los valores de energía no pueden ser negativos");
            }

            // Validar que la energía actual no supere la capacidad máxima
            if (battery.CurrentEnergyMWh >
                battery.MaximumCapacityMWh)
            {
                throw new Exception("La energía actual no puede superar la capacidad máxima de la batería");
            }

            // Validar el estado de la batería
            if (!IsValidStatus(battery.Status))
            {
                throw new Exception("El estado de la batería no es válido");
            }

            var batteryCrud = new BatteriesCrudFactory();

            // Obtener la batería registrada
            var currentBattery = batteryCrud.RetrieveById<Battery>(battery.Id);

            if (currentBattery == null)
            {
                throw new Exception("La batería que desea actualizar no existe");
            }

            // Validar que la batería no esté inactiva
            if (currentBattery.Status == "Inactive")
            {
                throw new Exception("No se puede modificar una batería inactiva");
            }

            var turbineCrud = new TurbineCrudFactory();

            // Obtener la turbina asociada a la batería
            var turbine = turbineCrud.RetrieveById<Turbine>(battery.TurbineId);

            // Validar que la turbina exista
            if (turbine == null)
            {
                throw new Exception("La turbina seleccionada no existe");
            }

            // Validar que la turbina no esté dada de baja
            if (turbine.Status == "Decommissioned")
            {
                throw new Exception("No se puede asignar la batería a una turbina dada de baja");
            }

            // Obtener todas las baterías registradas
            var batteries = batteryCrud.RetrieveAll<Battery>();

            // Validar que la turbina no tenga otra batería asignada
            foreach (var current in batteries)
            {
                if (current.Id != battery.Id &&
                    current.TurbineId == battery.TurbineId &&
                    current.Status != "Inactive")
                {
                    throw new Exception(
                        "La turbina seleccionada ya tiene otra batería asignada");
                }
            }

            // Evitar que los acumulados disminuyan
            if (battery.TotalGeneratedMWh <
                    currentBattery.TotalGeneratedMWh ||
                battery.TotalTransferredMWh <
                    currentBattery.TotalTransferredMWh ||
                battery.TotalSaturationLossMWh <
                    currentBattery.TotalSaturationLossMWh)
            {
                throw new Exception("Los valores acumulados de la batería no pueden disminuir");
            }

            // Mantener la fecha original de creación
            battery.CreatedAt = currentBattery.CreatedAt;

            // Asignar la fecha de actualización
            battery.UpdatedAt = DateTime.Now;

            batteryCrud.Update(battery);
        }


        public void Delete(Battery battery)
        {
            if (battery == null)
            {
                throw new Exception("La batería no puede ser nula");
            }

            if (battery.Id <= 0)
            {
                throw new Exception("El identificador de la batería no es válido");
            }

            var batteryCrud = new BatteriesCrudFactory();

            // Obtener la batería registrada
            var currentBattery = batteryCrud.RetrieveById<Battery>(battery.Id);

            if (currentBattery == null)
            {
                throw new Exception("La batería que desea desactivar no existe");
            }

            // Validar que la batería no esté inactiva
            if (currentBattery.Status == "Inactive")
            {
                throw new Exception("La batería ya se encuentra inactiva");
            }

            // Validar que la batería no tenga energía almacenada
            if (currentBattery.CurrentEnergyMWh > 0)
            {
                throw new Exception("No se puede desactivar una batería que todavía contiene energía");
            }

            // Asignar el estado inactivo
            battery.Status = "Inactive";

            // Asignar la fecha de actualización
            battery.UpdatedAt = DateTime.Now;

            // Desactivar lógicamente la batería
            batteryCrud.Delete(battery);
        }

        private bool HasEmptyFields(Battery battery)
        {
            return battery.TurbineId <= 0 ||
                   battery.MaximumCapacityMWh <= 0 ||
                   string.IsNullOrWhiteSpace(battery.Status);
        }

        private bool HasNegativeValues(Battery battery)
        {
            return battery.CurrentEnergyMWh < 0 ||
                   battery.TotalGeneratedMWh < 0 ||
                   battery.TotalTransferredMWh < 0 ||
                   battery.TotalSaturationLossMWh < 0;
        }

        private bool IsValidStatus(string status)
        {
            return status == "Active" || status == "Inactive";
        }
    }
}