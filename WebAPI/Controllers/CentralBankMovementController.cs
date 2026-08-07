using CoreApp;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentralBankMovementController : ControllerBase
    {
        [HttpGet]
        [Route("Retrieve")]
        public ActionResult Retrieve()
        {
            try
            {
                var manager = new CentralBankMovementManager();
                return Ok(manager.RetrieveAll());
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
                var manager = new CentralBankMovementManager();
                return Ok(manager.RetrieveById(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}