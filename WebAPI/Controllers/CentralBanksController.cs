using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentralBanksController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var cm = new CentralBankManager();
                var lstResults = cm.RetrieveAllCentralBanks();
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
                var cm = new CentralBankManager();
                var centralBank = cm.RetrieveById(id);
                return Ok(centralBank);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(CentralBank centralBank, [FromQuery] int? callerUserId)
        {
            try
            {
                var cm = new CentralBankManager();
                cm.Create(centralBank);
                AuditHelper.TryAudit(callerUserId, "Create", "CentralBank", centralBank.Id, "Banco central creado");
                return Ok(centralBank);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(CentralBank centralBank, [FromQuery] int? callerUserId)
        {
            try
            {
                var cm = new CentralBankManager();
                cm.Update(centralBank);
                AuditHelper.TryAudit(callerUserId, "Update", "CentralBank", centralBank.Id, "Banco central actualizado");
                return Ok(centralBank);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(CentralBank centralBank, [FromQuery] int? callerUserId)
        {
            try
            {
                var cm = new CentralBankManager();
                cm.Delete(centralBank);
                AuditHelper.TryAudit(callerUserId, "LogicalDelete", "CentralBank", centralBank.Id, "Banco central desactivado");
                return Ok(centralBank);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ReceiveEnergy/{id}/{mwh}")]
        public ActionResult ReceiveEnergy(int id, decimal mwh)
        {
            try
            {
                var cm = new CentralBankManager();
                cm.ReceiveEnergy(id, mwh);
                return Ok(new { message = "Energia recibido con exito" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Route("DistributeEnergy/{id}/{mwh}")]
        public ActionResult DistributeEnergey(int id, decimal mwh)
        {
            try
            {
                var cm = new CentralBankManager();
                cm.DistributeEnergy(id, mwh);
                return Ok(new { message = "Energia distribuida con exito" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
