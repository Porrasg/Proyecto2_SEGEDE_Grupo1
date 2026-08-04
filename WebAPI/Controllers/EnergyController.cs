using Microsoft.AspNetCore.Mvc;
using CoreApp;
using Entities_DTOs;
using System.Security.Cryptography.X509Certificates;

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


        
        [HttpGet]
        [Route("GenerationHistory/{turbineId}")]

        public ActionResult GenerationHistory(int turbineId)
        {
            try
            {
                var energyManager = new EnergyManager();

                var generationHistory = energyManager.RetrieveGenerationHistory(turbineId);

                return Ok(generationHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("LossHistory/{turbineId}")]

        public ActionResult LossHistory(int turbineId) 
        {
            try
            {
                var energyManager = new EnergyManager();
                var lossHistory = energyManager.RetrieveLossHistory(turbineId);

                return Ok(lossHistory);

            }
            catch (Exception ex) 
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }


        [HttpPost]
        [Route("CreateGeneration")]

        public ActionResult CreateGeneration(EnergyGeneration generation) 
        {
            try
            {
                //Se crea la instancia del manager
                var energyManager = new EnergyManager();
                // Se llama al metodo para crear la generacion de energia
                energyManager.CreateGeneration(generation);

                return Ok(generation);
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
