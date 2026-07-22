using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FailuresController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var fm = new FailureManager();
                var lstResults = fm.RetrieveAllFailures();
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
                var fm = new FailureManager();
                var failure = fm.RetrieveById(id);
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByTurbineId/{turbineId}")]
        public ActionResult RetrieveByTurbineId(int turbineId)
        {
            try
            {
                var fm = new FailureManager();
                var lstResults = fm.RetrieveByTurbineId(turbineId);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByEngineerId/{engineerId}")]
        public ActionResult RetrieveByEngineerId(int engineerId)
        {
            try
            {
                var fm = new FailureManager();
                var lstResults = fm.RetrieveByEngineerId(engineerId);
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
                var fm = new FailureManager();
                var lstResults = fm.RetrieveByStatus(status);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveBySeverity/{severity}")]
        public ActionResult RetrieveBySeverity(string severity)
        {
            try
            {
                var fm = new FailureManager();
                var lstResults = fm.RetrieveBySeverity(severity);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Failure failure)
        {
            try
            {
                var fm = new FailureManager();
                fm.Create(failure);
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Failure failure)
        {
            try
            {
                var fm = new FailureManager();
                fm.Update(failure);
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Failure failure)
        {
            try
            {
                var fm = new FailureManager();
                fm.Delete(failure);
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
