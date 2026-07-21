using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class CentralBankManager
    {

        public List<CentralBank> RetrieveAllCentralBanks()
        {
            var crud = new CentralBankCrudFactory();

            return crud.RetrieveAll<CentralBank>();
        }

        public CentralBank RetrieveById(int id)
        {
            var crud = new CentralBankCrudFactory();

            return crud.RetrieveById<CentralBank>(id);
        }

        public void Create(CentralBank bank)
        {

            if (HasEmptyFields(bank))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }


            if (bank.MaximumCapacityMWh <= 0)
            {
                throw new Exception("La capacidad máxima debe ser mayor a 0");
            }


            if (HasNegativeValues(bank))
            {
                throw new Exception("Los valores de energía no pueden ser negativos");
            }


            var crud = new CentralBankCrudFactory();

            crud.Create(bank);
        }

        public void Update(CentralBank bank)
        {
            var crud = new CentralBankCrudFactory();

            crud.Update(bank);
        }
        public void Delete(CentralBank bank)
        {
            var crud = new CentralBankCrudFactory();

            crud.Delete(bank);
        }
        private bool HasEmptyFields(CentralBank bank)
        {
            return string.IsNullOrWhiteSpace(bank.Name) ||
                   string.IsNullOrWhiteSpace(bank.Status);
        }
        private bool HasNegativeValues(CentralBank bank)
        {
            return bank.CurrentInventoryMWh < 0 ||
                   bank.TotalReceivedMWh < 0 ||
                   bank.TotalDistributedMWh < 0 ||
                   bank.TotalSaturationLossMWh < 0;
        }

    }
}
