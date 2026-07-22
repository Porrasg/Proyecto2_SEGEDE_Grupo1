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
            if (id <= 0)
            {
                throw new Exception("El identificador del forecast no es válido");
            }

            var forecastCrud = new ForecastCrudFactory();

            // Obtener el forecast registrado
            var forecast = forecastCrud.RetrieveById<Forecast>(id);

            // Validar que exista
            if (forecast == null)
            {
                throw new Exception("No se encontró el forecast solicitado");
            }

            return forecast;
        }


        public void Create(Forecast forecast)
        {
            if (forecast == null)
            {
                throw new Exception("El forecast no puede ser nulo");
            }

            // Asignar el estado inicial
            forecast.Status = "Pending";

            if (HasEmptyFields(forecast))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar el mes
            if (!IsValidMonth(forecast.ForecastMonth))
            {
                throw new Exception("El mes debe estar entre 1 y 12");
            }

            // Validar el periodo
            if (IsPastPeriod(forecast.ForecastYear, forecast.ForecastMonth))
            {
                throw new Exception("No se puede registrar un forecast para un periodo anterior al actual");
            }

            // Validar la energía solicitada
            if (forecast.RequestedEnergyMWh <= 0)
            {
                throw new Exception("La energía solicitada debe ser mayor a cero");
            }

            var forecastCrud = new ForecastCrudFactory();

            // Buscar un forecast del comprador para el mismo periodo
            var existingForecast = forecastCrud.RetrieveByBuyerPeriod(forecast.BuyerId, forecast.ForecastYear, forecast.ForecastMonth);

            // Validar duplicados
            if (existingForecast != null &&
                existingForecast.Status != "Cancelled")
            {
                throw new Exception("El comprador ya tiene un forecast registrado para el periodo indicado");
            }

            // Asignar fechas iniciales
            forecast.CreatedAt = DateTime.Now;
            forecast.UpdatedAt = null;

            // Crear el forecast
            forecastCrud.Create(forecast);
        }


        public void Update(Forecast forecast)
        {
            if (forecast == null)
            {
                throw new Exception("El forecast no puede ser nulo");
            }

            if (forecast.Id <= 0)
            {
                throw new Exception("El identificador del forecast no es válido");
            }

            if (HasEmptyFields(forecast))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar el mes
            if (!IsValidMonth(forecast.ForecastMonth))
            {
                throw new Exception("El mes debe estar entre 1 y 12");
            }

            // Validar la energía solicitada
            if (forecast.RequestedEnergyMWh <= 0)
            {
                throw new Exception("La energía solicitada debe ser mayor a cero");
            }

            // Validar el estado
            if (!IsValidStatus(forecast.Status))
            {
                throw new Exception("El estado del forecast no es válido");
            }

            var forecastCrud = new ForecastCrudFactory();

            // Obtener el forecast actual
            var currentForecast =forecastCrud.RetrieveById<Forecast>(forecast.Id);

            // Validar que exista
            if (currentForecast == null)
            {
                throw new Exception("El forecast que desea actualizar no existe");
            }

            // No permitir modificar forecasts procesados
            if (currentForecast.Status == "Processed")
            {
                throw new Exception("No se puede modificar un forecast que ya fue procesado");
            }

            // No permitir modificar forecasts cancelados
            if (currentForecast.Status == "Cancelled")
            {
                throw new Exception("No se puede modificar un forecast cancelado");
            }

            // Validar que el periodo no sea anterior
            if (IsPastPeriod(forecast.ForecastYear, forecast.ForecastMonth))
            {
                throw new Exception("No se puede asignar un periodo anterior al actual");
            }

            // Buscar otro forecast para el mismo comprador y periodo
            var existingForecast = forecastCrud.RetrieveByBuyerPeriod(forecast.BuyerId, forecast.ForecastYear, forecast.ForecastMonth);

            // Validar que no sea otro registro
            if (existingForecast != null &&
                existingForecast.Id != forecast.Id &&
                existingForecast.Status != "Cancelled")
            {
                throw new Exception("El comprador ya tiene otro forecast registrado para el periodo indicado");
            }

            // Mantener la fecha original
            forecast.CreatedAt = currentForecast.CreatedAt;

            // Asignar fecha de actualización
            forecast.UpdatedAt = DateTime.Now;

            // Actualizar el forecast
            forecastCrud.Update(forecast);
        }


        public void Delete(Forecast forecast)
        {
            if (forecast == null)
            {
                throw new Exception("El forecast no puede ser nulo");
            }

            if (forecast.Id <= 0)
            {
                throw new Exception("El identificador del forecast no es válido");
            }

            var forecastCrud = new ForecastCrudFactory();

            // Obtener el forecast actual
            var currentForecast =forecastCrud.RetrieveById<Forecast>(forecast.Id);

            // Validar que exista
            if (currentForecast == null)
            {
                throw new Exception("El forecast que desea cancelar no existe");
            }

            // Validar que no se encuentre cancelado
            if (currentForecast.Status == "Cancelled")
            {
                throw new Exception( "El forecast ya se encuentra cancelado");
            }

            // No cancelar un forecast procesado
            if (currentForecast.Status == "Processed")
            {
                throw new Exception("No se puede cancelar un forecast que ya fue procesado");
            }

            // Asignar eliminación lógica
            forecast.Status = "Cancelled";
            forecast.UpdatedAt = DateTime.Now;

            forecastCrud.Delete(forecast);
        }

        public List<Forecast> RetrieveByBuyerId(int buyerId)
        {
            // Validar el identificador del comprador
            if (buyerId <= 0)
            {
                throw new Exception("El identificador del comprador no es válido");
            }

            var forecastCrud = new ForecastCrudFactory();

            return forecastCrud.RetrieveByBuyerId(buyerId);
        }

        // Metodo que permite obtener todos los forecasts de un periodo especifico (año y mes)
        public List<Forecast> RetrieveByPeriod(int forecastYear, int forecastMonth)
        {
            // Validar el mes
            if (!IsValidMonth(forecastMonth))
            {
                throw new Exception("El mes debe estar entre 1 y 12");
            }

            // Validar el año
            if (forecastYear <= 0)
            {
                throw new Exception("El año del forecast no es válido");
            }

            var forecastCrud = new ForecastCrudFactory();

            return forecastCrud.RetrieveByPeriod(forecastYear, forecastMonth);
        }

        // Metodo que permite obtener un forecast de un comprador especifico en un periodo especifico (año y mes)
        public Forecast RetrieveByBuyerPeriod(int buyerId, int forecastYear, int forecastMonth)
        {
            // Validar el comprador
            if (buyerId <= 0)
            {
                throw new Exception("El identificador del comprador no es válido");
            }

            // Validar el mes
            if (!IsValidMonth(forecastMonth))
            {
                throw new Exception("El mes debe estar entre 1 y 12");
            }

            // Validar el año
            if (forecastYear <= 0)
            {
                throw new Exception("El año del forecast no es válido");
            }

            var forecastCrud = new ForecastCrudFactory();

            return forecastCrud.RetrieveByBuyerPeriod(buyerId, forecastYear, forecastMonth);
        }

        // Metodo que permite obtener todos los forecasts con un estado especifico
        public List<Forecast> RetrieveByStatus(string status)
        {
            // Validar el estado
            if (!IsValidStatus(status))
            {
                throw new Exception("El estado del forecast no es válido");
            }

            var forecastCrud = new ForecastCrudFactory();

            return forecastCrud.RetrieveByStatus(status);
        }


        private bool HasEmptyFields(Forecast forecast)
        {
            return forecast.BuyerId <= 0 || string.IsNullOrWhiteSpace(forecast.Status);
        }

        private bool IsValidMonth(int month)
        {
            return month >= 1 && month <= 12;
        }

        private bool IsValidStatus(string status)
        {
            return status == "Pending" ||
                   status == "Processed" ||
                   status == "Cancelled";
        }

        private bool IsPastPeriod(int year, int month)
        {
            var currentDate = DateTime.Now;

            return year < currentDate.Year || (year == currentDate.Year && month < currentDate.Month);
        }
    }
}