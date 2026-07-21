using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class ForecastManager
    {

        public List<Forecast> RetrieveAllForecasts()
        {
            var crud = new ForecastCrudFactory();

            return crud.RetrieveAll<Forecast>();
        }


        public Forecast RetrieveById(int id)
        {
            var crud = new ForecastCrudFactory();

            return crud.RetrieveById<Forecast>(id);
        }


        public void Create(Forecast forecast)
        {

            if (HasEmptyFields(forecast))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }


            if (!IsValidMonth(forecast))
            {
                throw new Exception("El mes debe estar entre 1 y 12");
            }


            if (forecast.ForecastYear < DateTime.Now.Year)
            {
                throw new Exception("El año del forecast no puede ser anterior al actual");
            }


            if (forecast.RequestedEnergyMWh <= 0)
            {
                throw new Exception("La energía solicitada debe ser mayor a 0");
            }


            var crud = new ForecastCrudFactory();

            crud.Create(forecast);
        }

        public void Update(Forecast forecast)
        {
            var crud = new ForecastCrudFactory();

            crud.Update(forecast);
        }

        public void Delete(Forecast forecast)
        {
            var crud = new ForecastCrudFactory();

            crud.Delete(forecast);
        }
        private bool HasEmptyFields(Forecast f)
        {
            return f.BuyerId <= 0 ||
                   string.IsNullOrWhiteSpace(f.Status);
        }

        private bool IsValidMonth(Forecast f)
        {
            return f.ForecastMonth >= 1 &&
                   f.ForecastMonth <= 12;
        }

    }
}