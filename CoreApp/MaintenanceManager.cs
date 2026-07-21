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
        public Maintenance RetrieveById(int id)
        {
            var crud = new MaintenanceCrudFactory();

            return crud.RetrieveById<Maintenance>(id);
        }
        public void Create(Maintenance maintenance)
        {

            if (HasEmptyFields(maintenance))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }


            if (maintenance.EstimatedEndDate < maintenance.EstimatedStartDate)
            {
                throw new Exception("La fecha final no puede ser anterior a la inicial");
            }


            if (maintenance.ActualEndDate.HasValue &&
                maintenance.ActualStartDate.HasValue &&
                maintenance.ActualEndDate < maintenance.ActualStartDate)
            {
                throw new Exception("La fecha real final no puede ser anterior a la inicial");
            }

            var crud = new MaintenanceCrudFactory();

            crud.Create(maintenance);
        }
        public void Update(Maintenance maintenance)
        {
            var crud = new MaintenanceCrudFactory();

            crud.Update(maintenance);
        }

        public void Delete(Maintenance maintenance)
        {
            var crud = new MaintenanceCrudFactory();

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

    }
}