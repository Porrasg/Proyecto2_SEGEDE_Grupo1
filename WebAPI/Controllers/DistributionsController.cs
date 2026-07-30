using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributionsController : ControllerBase
    {
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
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByBuyerId/{buyerId}")]
        [Route("ByBuyer/{buyerId}")] // alias que usa ReportsViewController.js (mismo patrón que ForecastsController)
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
                AuditHelper.TryAudit(callerUserId, "Create", "Distributions", distribution.Id, "Distribución de energía creada");
                return Ok(distribution);
            }
            catch (Exception ex)
            {
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
                return StatusCode(500, ex.Message);
            }
        }
    }
}
