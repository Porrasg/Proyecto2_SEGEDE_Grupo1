using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForecastsController : ControllerBase
    {
        // Cuerpo del registro de pronóstico que envía la pantalla del comprador
        public class ForecastRegisterRequest
        {
            public int BuyerId { get; set; }
            public int ForecastMonth { get; set; }
            public int ForecastYear { get; set; }
            public decimal RequestedEnergyMWh { get; set; }
        }

        // Cuerpo de la modificación de un pronóstico existente
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

        // Pronósticos de un mes/año específico (pantalla Admin/Forecasts)
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

        // Las solicitudes de compra son "Interfaz exclusiva para usuarios Distribuidores"
        // (rúbrica). La UI ya oculta estas pantallas a otros roles, pero eso es solo
        // cosmético del lado del cliente; esta validación es la que realmente lo exige
        // en el servidor. No hay autenticación por token en el proyecto (sesión vive en
        // sessionStorage del navegador), así que sigue siendo el mismo modelo de
        // confianza que el resto de la API - pero al menos ya no basta con ocultar el
        // botón en el frontend para saltarse la regla.
        private static ActionResult ValidateDistributorRole(string callerRole)
        {
            // El propio frontend (BuyerManagementViewController.js) ya permite Distributor
            // Y Administrator/Admin en esta pantalla (soporte/gestión en nombre del comprador);
            // la validación del backend debe reflejar exactamente esa misma regla.
            bool isAllowed = string.Equals(callerRole, "Distributor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(callerRole, "Administrator", StringComparison.OrdinalIgnoreCase)
                || string.Equals(callerRole, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAllowed)
            {
                return new ObjectResult(new { message = "Esta operación es exclusiva para usuarios Distribuidores o Administradores." })
                {
                    StatusCode = 403
                };
            }

            return null;
        }

        // Registro de un pronóstico nuevo desde la pantalla del comprador.
        // El manager asigna el estado inicial "Pending" y valida duplicados y periodos pasados.
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

        // Modifica únicamente la energía solicitada de un pronóstico existente.
        // Se recupera el registro actual para conservar el resto de sus datos y
        // que el manager aplique sus validaciones (no procesado, no cancelado).
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

        // Cancela (baja lógica) un pronóstico. El manager valida que no esté
        // procesado ni cancelado y asigna el estado "Cancelled".
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
