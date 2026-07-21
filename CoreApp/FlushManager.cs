using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class FlushManager
    {

        public List<Flush> RetrieveAllFlushes()
        {
            var crud = new FlushCrudFactory();

            return crud.RetrieveAll<Flush>();
        }
        public Flush RetrieveById(int id)
        {
            var crud = new FlushCrudFactory();

            return crud.RetrieveById<Flush>(id);
        }

        public void Create(Flush flush)
        {

            if (HasEmptyFields(flush))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }


            if (HasNegativeEnergy(flush))
            {
                throw new Exception("Los valores de energía no pueden ser negativos");
            }


            var crud = new FlushCrudFactory();

            crud.Create(flush);
        }

        public void Update(Flush flush)
        {
            var crud = new FlushCrudFactory();

            crud.Update(flush);
        }

        public void Delete(Flush flush)
        {
            var crud = new FlushCrudFactory();

            crud.Delete(flush);
        }
        private bool HasEmptyFields(Flush f)
        {
            return f.FlushBatchId <= 0 ||
                   f.TurbineId <= 0 ||
                   f.BatteryId <= 0 ||
                   f.CentralBankId <= 0 ||
                   string.IsNullOrWhiteSpace(f.ExecutionType) ||
                   string.IsNullOrWhiteSpace(f.Status);
        }
        private bool HasNegativeEnergy(Flush f)
        {
            return f.SnapshotEnergyMWh < 0 ||
                   f.TransferredEnergyMWh < 0 ||
                   f.SaturationLossMWh < 0;
        }

    }
}
