using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatteriesController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveAllBatteries")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var bm = new BatteryManager();
                var lstResults = bm.RetrieveAllBatteries();
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
                var bm = new BatteryManager();
                var battery = bm.RetrieveById(id);
                return Ok(battery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Battery battery, [FromQuery] int? callerUserId)
        {
            try
            {
                var bm = new BatteryManager();
                bm.Create(battery);
                AuditHelper.TryAudit(callerUserId, "Create", "Batteries", battery.Id, "Batería creada");
                return Ok(battery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Battery battery, [FromQuery] int? callerUserId)
        {
            try
            {
                var bm = new BatteryManager();
                bm.Update(battery);
                AuditHelper.TryAudit(callerUserId, "Update", "Batteries", battery.Id, "Batería actualizada");
                return Ok(battery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Battery battery, [FromQuery] int? callerUserId)
        {
            try
            {
                var bm = new BatteryManager();
                bm.Delete(battery);
                AuditHelper.TryAudit(callerUserId, "LogicalDelete", "Batteries", battery.Id, "Batería desactivada");
                return Ok(battery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
