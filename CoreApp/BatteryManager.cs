using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class BatteryManager
    {
        public List<Battery> RetrieveAllBatteries()
        {
            var crud = new BatteryCrudFactory();

            return crud.RetrieveAll<Battery>();
        }


        public Battery RetrieveById(int id)
        {
            var crud = new BatteryCrudFactory();

            return crud.RetrieveById<Battery>(id);
        }

        public void Create(Battery battery)
        {

            if (HasEmptyFields(battery))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }


            if (battery.MaximumCapacityMWh <= 0)
            {
                throw new Exception("La capacidad máxima debe ser mayor a 0");
            }


            if (HasNegativeValues(battery))
            {
                throw new Exception("Los valores de energía no pueden ser negativos");
            }


            var crud = new BatteryCrudFactory();

            crud.Create(battery);
        }


        public void Update(Battery battery)
        {
            var crud = new BatteryCrudFactory();

            crud.Update(battery);
        }
        public void Delete(Battery battery)
        {
            var crud = new BatteryCrudFactory();

            crud.Delete(battery);
        }

        private bool HasEmptyFields(Battery battery)
        {
            return battery.TurbineId <= 0 ||
                   string.IsNullOrWhiteSpace(battery.Status);
        }

        private bool HasNegativeValues(Battery battery)
        {
            return battery.CurrentEnergyMWh < 0 ||
                   battery.TotalGeneratedMWh < 0 ||
                   battery.TotalTransferredMWh < 0 ||
                   battery.TotalSaturationLossMWh < 0;
        }

    }
}