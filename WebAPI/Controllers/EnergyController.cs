using CoreApp;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnergyController : ControllerBase
    {
        // Historial de producción neta de una turbina. Paginado en memoria porque el
        // volumen de cortes de un proyecto de este alcance no justifica paginar en el SP;
        // ReportsViewController.js ya espera exactamente esta forma de respuesta
        // ({ data: { items, totalPages } }) para consolidar Energía Generada por turbina
        // y Energía Suplida por Proveedor.
        [HttpGet]
        [Route("GenerationHistory/{turbineId}")]
        public ActionResult GenerationHistory(int turbineId, [FromQuery] int page = 1, [FromQuery] int pageSize = 1000)
        {
            try
            {
                var em = new EnergyManager();
                var all = em.RetrieveGenerationHistory(turbineId);

                var totalPages = pageSize > 0 ? (int)System.Math.Ceiling(all.Count / (double)pageSize) : 1;
                if (totalPages < 1) totalPages = 1;

                var items = all
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new { data = new { items, totalPages } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Ejecuta el corte de producción (diario/semanal) para todas las turbinas activas:
        // calcula la producción neta desde el último corte y la acredita a cada batería.
        [HttpPost]
        [Route("ExecutePeriodProduction")]
        public ActionResult ExecutePeriodProduction([FromQuery] int? callerUserId)
        {
            try
            {
                var em = new EnergyManager();
                var results = em.ExecutePeriodProduction();

                AuditHelper.TryAudit(callerUserId, "Execute", "EnergyProduction", null,
                    $"Corte de producción ejecutado sobre {results.Count} turbina(s)");

                return Ok(new
                {
                    message = $"Producción calculada para {results.Count} turbina(s).",
                    data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
