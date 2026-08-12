using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;


namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionCutController : ControllerBase
    {
        [HttpPost("execute/{centralBankId}")]
        public IActionResult ExecuteProductionCut(int centralBankId)
        {
            try
            {
                var manager = new ProductionCutManager();

                manager.ExecuteProductionCut(centralBankId);

                return Ok(new
                {
                    message = "Corte de producción ejecutado correctamente."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}