using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForecastsController : ControllerBase
    {
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
    }
}
