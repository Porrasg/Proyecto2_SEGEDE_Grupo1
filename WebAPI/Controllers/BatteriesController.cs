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
        public ActionResult Create(Battery battery)
        {
            try
            {
                var bm = new BatteryManager();
                bm.Create(battery);
                return Ok(battery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Battery battery)
        {
            try
            {
                var bm = new BatteryManager();
                bm.Update(battery);
                return Ok(battery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Battery battery)
        {
            try
            {
                var bm = new BatteryManager();
                bm.Delete(battery);
                return Ok(battery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
