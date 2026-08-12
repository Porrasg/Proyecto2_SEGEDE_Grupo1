using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForecastsController : ControllerBase
    {
        // Datos que manda la pantalla del comprador cuando registra un pronóstico.
        public class ForecastRegisterRequest
        {
            public int BuyerId { get; set; }
            public int ForecastMonth { get; set; }
            public int ForecastYear { get; set; }
            public decimal RequestedEnergyMWh { get; set; }
        }

        // Datos que manda la pantalla cuando modifica un pronóstico ya guardado.
        public class ForecastModifyRequest
        {
            public int ForecastId { get; set; }
            public decimal NewRequestedEnergyMWh { get; set; }
        }

        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var fm = new ForecastManager();
                var lstResults = fm.RetrieveAllForecasts();
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveById/{id}")]
        public ActionResult RetrieveById(int id)
        {
            try
            {
                var fm = new ForecastManager();
                var forecast = fm.RetrieveById(id);
                return Ok(forecast);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByBuyerId/{buyerId}")]
        [Route("ByBuyer/{buyerId}")] // alias que usan las pantallas del comprador y Reportes
        public ActionResult RetrieveByBuyerId(int buyerId)
        {
            try
            {
                var fm = new ForecastManager();
                var lstResults = fm.RetrieveByBuyerId(buyerId);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByStatus/{status}")]
        public ActionResult RetrieveByStatus(string status)
        {
            try
            {
                var fm = new ForecastManager();
                var lstResults = fm.RetrieveByStatus(status);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Forecast forecast)
        {
            try
            {
                var fm = new ForecastManager();
                fm.Create(forecast);
                return Ok(forecast);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Forecast forecast)
        {
            try
            {
                var fm = new ForecastManager();
                fm.Update(forecast);
                return Ok(forecast);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Forecast forecast)
        {
            try
            {
                var fm = new ForecastManager();
                fm.Delete(forecast);
                return Ok(forecast);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Pronósticos de un mes y año específico para la pantalla de administración.
        [HttpGet]
        [Route("ByMonth")]
        public ActionResult ByMonth([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var fm = new ForecastManager();
                var lstResults = fm.RetrieveByPeriod(year, month);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Aquí valido desde el servidor qué roles sí pueden trabajar con pronósticos.
        private static ActionResult ValidateDistributorRole(string callerRole)
        {
            // Acepto Buyer y algunos roles cercanos porque así está manejado en la app.
            bool isAllowed = string.Equals(callerRole, "Buyer", StringComparison.OrdinalIgnoreCase)
                || string.Equals(callerRole, "Customer", StringComparison.OrdinalIgnoreCase)
                || string.Equals(callerRole, "Distributor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(callerRole, "Administrator", StringComparison.OrdinalIgnoreCase)
                || string.Equals(callerRole, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAllowed)
            {
                return new ObjectResult(new { message = "Esta operación es exclusiva para usuarios Comprador, Distribuidor o Administrador." })
                {
                    StatusCode = 403
                };
            }

            return null;
        }

        // Registra un pronóstico nuevo desde la pantalla del comprador.
        // El manager se encarga del estado inicial y de revisar duplicados o periodos inválidos.
        [HttpPost]
        [Route("Register")]
        public ActionResult Register(ForecastRegisterRequest request, [FromQuery] int? callerUserId, [FromQuery] string callerRole)
        {
            var roleError = ValidateDistributorRole(callerRole);
            if (roleError != null) return roleError;

            try
            {
                var fm = new ForecastManager();
                var forecast = new Forecast
                {
                    BuyerId = request.BuyerId,
                    ForecastMonth = request.ForecastMonth,
                    ForecastYear = request.ForecastYear,
                    RequestedEnergyMWh = request.RequestedEnergyMWh
                };

                fm.Create(forecast);
                AuditHelper.TryAudit(callerUserId ?? request.BuyerId, "Create", "Forecasts", forecast.Id,
                    $"Solicitud de compra registrada: {request.RequestedEnergyMWh} MWh para {request.ForecastMonth}/{request.ForecastYear}");
                return Ok(new { message = "Pronóstico registrado con éxito.", data = forecast });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Modifica solo la energía solicitada del pronóstico.
        // Primero recupero el registro para no perder los demás datos.
        [HttpPut]
        [Route("Modify")]
        public ActionResult Modify(ForecastModifyRequest request, [FromQuery] int? callerUserId, [FromQuery] string callerRole)
        {
            var roleError = ValidateDistributorRole(callerRole);
            if (roleError != null) return roleError;

            try
            {
                var fm = new ForecastManager();
                var forecast = fm.RetrieveById(request.ForecastId);

                forecast.RequestedEnergyMWh = request.NewRequestedEnergyMWh;

                fm.Update(forecast);
                AuditHelper.TryAudit(callerUserId, "Update", "Forecasts", forecast.Id,
                    $"Solicitud de compra modificada a {request.NewRequestedEnergyMWh} MWh");
                return Ok(new { message = "Pronóstico actualizado con éxito.", data = forecast });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Cancela el pronóstico de forma lógica, sin borrarlo físicamente.
        [HttpPost]
        [Route("Cancel/{id}")]
        public ActionResult Cancel(int id, [FromQuery] int? callerUserId, [FromQuery] string callerRole)
        {
            var roleError = ValidateDistributorRole(callerRole);
            if (roleError != null) return roleError;

            try
            {
                var fm = new ForecastManager();
                var forecast = fm.RetrieveById(id);

                fm.Delete(forecast);
                AuditHelper.TryAudit(callerUserId, "LogicalDelete", "Forecasts", id, "Solicitud de compra cancelada");
                return Ok(new { message = "Pronóstico cancelado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
