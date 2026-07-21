using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class DistributionManager
    {

        public List<Distribution> RetrieveAllDistributions()
        {
            var crud = new DistributionCrudFactory();

            return crud.RetrieveAll<Distribution>();
        }
        public Distribution RetrieveById(int id)
        {
            var crud = new DistributionCrudFactory();

            return crud.RetrieveById<Distribution>(id);
        }

        public void Create(Distribution distribution)
        {

            if (HasEmptyFields(distribution))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }


            if (HasNegativeEnergy(distribution))
            {
                throw new Exception("Los valores de energía no pueden ser negativos");
            }


            if (distribution.AssignedEnergyMWh > distribution.RequestedEnergyMWh)
            {
                throw new Exception("La energía asignada no puede superar la solicitada");
            }


            if (distribution.UnitPrice <= 0)
            {
                throw new Exception("El precio unitario debe ser mayor a 0");
            }

            var crud = new DistributionCrudFactory();

            crud.Create(distribution);
        }
        public void Update(Distribution distribution)
        {
            var crud = new DistributionCrudFactory();

            crud.Update(distribution);
        }
        public void Delete(Distribution distribution)
        {
            var crud = new DistributionCrudFactory();

            crud.Delete(distribution);
        }
        private bool HasEmptyFields(Distribution d)
        {
            return d.DistributionBatchId <= 0 ||
                   d.ForecastId <= 0 ||
                   d.BuyerId <= 0 ||
                   d.CentralBankId <= 0 ||
                   string.IsNullOrWhiteSpace(d.Status);
        }
        private bool HasNegativeEnergy(Distribution d)
        {
            return d.RequestedEnergyMWh < 0 ||
                   d.AssignedEnergyMWh < 0 ||
                   d.UnassignedEnergyMWh < 0;
        }

    }
}
