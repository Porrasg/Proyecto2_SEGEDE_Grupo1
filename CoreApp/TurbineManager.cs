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


        public void Create(Turbine t)
        {
            if (HasEmptyFields(t))
            {
                throw new Exception("Todos los campos obligatorios deben estar completos");
            }


            if (t.ManufactureYear > DateTime.Now.Year)
            {
                throw new Exception("El año de fabricación no puede ser futuro");
            }


            if (t.NominalWeeklyCapacityMWh <= 0)
            {
                throw new Exception("La capacidad nominal debe ser mayor a 0");
            }


            if (!IsValidStatus(t))
            {
                throw new Exception("El estado debe ser AC o IN");
            }


            var tCrud = new TurbineCrudFactory();

            tCrud.Create(t);
        }

        public void Update(Turbine t)
        {
            var tCrud = new TurbineCrudFactory();

            tCrud.Update(t);
        }
        public void Delete(Turbine t)
        {
            var tCrud = new TurbineCrudFactory();

            tCrud.Delete(t);
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
        private bool IsValidStatus(Turbine turbine)
        {
            return turbine.Status == "Activa" ||
                   turbine.Status == "Inactiva";
        }
    }
}