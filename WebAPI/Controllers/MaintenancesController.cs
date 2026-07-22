using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenancesController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var mm = new MaintenanceManager();
                var lstResults = mm.RetrieveAllMaintenances();
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
                var mm = new MaintenanceManager();
                var maintenance = mm.RetrieveById(id);
                return Ok(maintenance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Maintenance maintenance)
        {
            try
            {
                var mm = new MaintenanceManager();
                mm.Create(maintenance);
                return Ok(maintenance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Maintenance maintenance)
        {
            try
            {
                var mm = new MaintenanceManager();
                mm.Update(maintenance);
                return Ok(maintenance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Maintenance maintenance)
        {
            try
            {
                var mm = new MaintenanceManager();
                mm.Delete(maintenance);
                return Ok(maintenance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
