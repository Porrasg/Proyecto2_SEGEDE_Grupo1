using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurbinesController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var tm = new TurbineManager();
                var lstResults = tm.RetrieveAllTurbines();
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Turbine turbine)
        {
            try
            {
                var tm = new TurbineManager();
                tm.Create(turbine);
                return Ok(turbine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Turbine turbine)
        {
            try
            {
                var tm = new TurbineManager();
                tm.Update(turbine);
                return Ok(turbine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Turbine turbine)
        {
            try
            {
                var tm = new TurbineManager();
                tm.Delete(turbine);
                return Ok(turbine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
