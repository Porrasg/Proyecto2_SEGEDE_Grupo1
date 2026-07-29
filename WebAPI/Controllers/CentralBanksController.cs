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
        public ActionResult Create(CentralBank centralBank)
        {
            try
            {
                var cm = new CentralBankManager();
                cm.Create(centralBank);
                return Ok(centralBank);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(CentralBank centralBank)
        {
            try
            {
                var cm = new CentralBankManager();
                cm.Update(centralBank);
                return Ok(centralBank);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(CentralBank centralBank)
        {
            try
            {
                var cm = new CentralBankManager();
                cm.Delete(centralBank);
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
