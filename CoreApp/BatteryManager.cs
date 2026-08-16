using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreApp
{
    public class BatteryManager
    {
        public List<Battery> RetrieveAllBatteries()
        {
            var batteryCrud = new BatteriesCrudFactory();
            return batteryCrud.RetrieveAll<Battery>() ?? new List<Battery>();
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

        //metodo para almacenar la energia, para cuando se implemente la simulacion de generacion de energia
        public decimal StoreEnergy(int batteryId, decimal generatedEnergy)
        {
            if (generatedEnergy <= 0)
            {
                throw new Exception("La energía generada debe ser mayor a cero.");
            }

            var batteryCrud = new BatteriesCrudFactory();
            var battery = batteryCrud.RetrieveById<Battery>(batteryId);

            if (battery == null)
            {
                throw new Exception("La batería no existe.");
            }

            var overflow = EnergyStorageCalculator.ApplyGeneratedEnergy(
                battery,
                generatedEnergy,
                DateTime.UtcNow);

            batteryCrud.Update(battery);

            return overflow;
        }

        public Battery SetCurrentEnergyByTurbine(int turbineId, decimal currentEnergyMWh)
        {
            if (turbineId <= 0)
            {
                throw new BusinessException("El identificador de la turbina no es válido");
            }

            var batteryCrud = new BatteriesCrudFactory();
            var battery = batteryCrud.RetrieveAll<Battery>()
                .FirstOrDefault(b => b.TurbineId == turbineId && b.Status == "Active");

            if (battery == null)
            {
                throw new BusinessException("La turbina no posee una batería activa");
            }

            if (currentEnergyMWh < 0 || currentEnergyMWh > battery.MaximumCapacityMWh)
            {
                throw new BusinessException($"La carga debe estar entre 0 y {battery.MaximumCapacityMWh} MWh");
            }

            battery.CurrentEnergyMWh = currentEnergyMWh;
            battery.UpdatedAt = DateTime.Now;
            batteryCrud.Update(battery);
            return battery;
        }

        public decimal TransferEnergyToCentralBank(
    int batteryId,
    int centralBankId)
        {
            if (batteryId <= 0)
            {
                throw new Exception("El identificador de la batería no es válido");
            }

            if (centralBankId <= 0)
            {
                throw new Exception("El identificador del banco central no es válido");
            }

            var batteryCrud = new BatteriesCrudFactory();

            var battery = batteryCrud.RetrieveById<Battery>(batteryId);

            if (battery == null)
            {
                throw new Exception("La bateria no existe");
            }

            if (battery.Status != "Active")
            {
                throw new Exception("La bateria debe estar activa");
            }

            if (battery.CurrentEnergyMWh <= 0)
            {
                throw new Exception(
                    "La batería no posee energia disponible para transferir");
            }

            decimal energyToTransfer = battery.CurrentEnergyMWh;

            // El Banco Central se encarga de validar su capacidad y registrar una posible saturación.
            decimal overflow = new CentralBankManager()
                .ReceiveEnergy(
                    centralBankId,
                    energyToTransfer);

            // Toda la energía sale físicamente de la batería.
            battery.CurrentEnergyMWh = 0;

            // Se registra la energía enviada desde la batería.
            battery.TotalTransferredMWh += energyToTransfer;

            battery.UpdatedAt = DateTime.UtcNow;

            batteryCrud.Update(battery);

            return overflow;
        }
    }
}
