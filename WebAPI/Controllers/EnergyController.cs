using Microsoft.AspNetCore.Mvc;
using CoreApp;
using Entities_DTOs;

namespace WebAPI.Controllers
{


    [Route("api/[controller]")]
    [ApiController]


    public class EnergyController : ControllerBase
    {

        [HttpGet]
        [Route("LocalBattery/{turbineId}")]

        public ActionResult LocalBattery(int turbineId) 
        {
            try 
            {
                var batteryManager = new BatteryManager();

                var batteries = batteryManager.RetrieveAllBatteries();

                var battery = batteries.FirstOrDefault(b => b.TurbineId == turbineId);

                if (battery == null) 
                {
                    return NotFound(new
                    {
                        message = "No se encontro una bateria asociada a la turbina"
                    });
                }

                return Ok(battery);

            }
            catch (Exception ex) 
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });            
            }         



        }

    }
}
