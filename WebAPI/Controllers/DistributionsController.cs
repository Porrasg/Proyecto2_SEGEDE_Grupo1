using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributionsController : ControllerBase
    {
        // Ejecuta el cierre mensual: reparte la energía disponible, crea las facturas y
        // descuenta lo asignado del Banco Central. Si no mandan banco, usa el primero.
        [HttpPost]
        [Route("ExecuteMonthly")]
        public ActionResult ExecuteMonthly([FromQuery] int year, [FromQuery] int month, [FromQuery] int? centralBankId, [FromQuery] int? callerUserId)
        {
            try
            {
                var cbId = centralBankId;
                if (cbId == null || cbId <= 0)
                {
                    var banks = new CentralBankManager().RetrieveAllCentralBanks();
                    if (banks == null || banks.Count == 0)
                    {
                        return StatusCode(500, new { message = "No hay ningún Banco Central registrado." });
                    }
                    cbId = banks[0].Id;
                }

                var dm = new DistributionManager();
                var results = dm.ExecuteMonthlyClosing(year, month, cbId.Value);

                // Dejo registro de que se hizo el cierre mensual.
                AuditHelper.TryAudit(callerUserId, "Execute", "Distributions", null,
                    $"Distribución mensual ejecutada para {month}/{year}: {results.Count} comprador(es)");

                return Ok(new { message = $"Distribución ejecutada con éxito para {results.Count} comprador(es).", data = results });
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(DistributionsController), ex, "ExecuteMonthly");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var dm = new DistributionManager();
                var lstResults = dm.RetrieveAllDistributions();
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(DistributionsController), ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveById/{id}")]
        public ActionResult RetrieveById(int id)
        {
            try
            {
                var dm = new DistributionManager();
                var distribution = dm.RetrieveById(id);
                return Ok(distribution);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(DistributionsController), ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByBatchId/{batchId}")]
        public ActionResult RetrieveByBatchId(int batchId)
        {
            try
            {
                var dm = new DistributionManager();
                var lstResults = dm.RetrieveByBatchId(batchId);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(DistributionsController), ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByBuyerId/{buyerId}")]
        [Route("ByBuyer/{buyerId}")] 
        // Alias que usa la pantalla de reportes para consultar por comprador.
        public ActionResult RetrieveByBuyerId(int buyerId)
        {
            try
            {
                var dm = new DistributionManager();
                var lstResults = dm.RetrieveByBuyerId(buyerId);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(DistributionsController), ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByStatus/{status}")]
        public ActionResult RetrieveByStatus(string status)
        {
            try
            {
                var dm = new DistributionManager();
                var lstResults = dm.RetrieveByStatus(status);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(DistributionsController), ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Distribution distribution, [FromQuery] int? callerUserId)
        {
            try
            {
                var dm = new DistributionManager();
                dm.Create(distribution);
                // Registro simple para auditoría.
                AuditHelper.TryAudit(callerUserId, "Create", "Distributions", distribution.Id, "Distribución de energía creada");
                return Ok(distribution);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(DistributionsController), ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Distribution distribution, [FromQuery] int? callerUserId)
        {
            try
            {
                var dm = new DistributionManager();
                dm.Update(distribution);
                AuditHelper.TryAudit(callerUserId, "Update", "Distributions", distribution.Id, "Distribución de energía actualizada");
                return Ok(distribution);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(DistributionsController), ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Distribution distribution, [FromQuery] int? callerUserId)
        {
            try
            {
                var dm = new DistributionManager();
                dm.Delete(distribution);
                AuditHelper.TryAudit(callerUserId, "LogicalDelete", "Distributions", distribution.Id, "Distribución de energía eliminada");
                return Ok(distribution);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(DistributionsController), ex);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
