using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class FailureManager
    {
        public List<Failure> RetrieveAllFailures()
        {
            var crud = new FailureCrudFactory();

            return crud.RetrieveAll<Failure>();
        }
        public Failure RetrieveById(int id)
        {
            var crud = new FailureCrudFactory();

            return crud.RetrieveById<Failure>(id);
        }
        public void Create(Failure failure)
        {

            if (HasEmptyFields(failure))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            if (failure.FailureDate > DateTime.Now)
            {
                throw new Exception("La fecha del fallo no puede ser futura");
            }

            var crud = new FailureCrudFactory();

            crud.Create(failure);
        }
        public void Update(Failure failure)
        {
            var crud = new FailureCrudFactory();

            crud.Update(failure);
        }
        public void Delete(Failure failure)
        {
            var crud = new FailureCrudFactory();

            crud.Delete(failure);
        }

        private bool HasEmptyFields(Failure f)
        {
            return f.TurbineId <= 0 ||
                   f.EngineerId <= 0 ||
                   string.IsNullOrWhiteSpace(f.Severity) ||
                   string.IsNullOrWhiteSpace(f.Description) ||
                   string.IsNullOrWhiteSpace(f.Status);
        }

    }
}
