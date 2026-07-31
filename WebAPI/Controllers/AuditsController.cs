using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditsController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var am = new AuditManager();
                var lstResults = am.RetrieveAllAudits();
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Bitácora filtrada por rango de fechas (pantalla Admin/Audit).
        // La respuesta se envuelve como { data: { items: [...] } } porque así la
        // lee el frontend; la paginación de la pantalla es en memoria.
        [HttpGet]
        [Route("ByDateRange")]
        public ActionResult ByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            try
            {
                var am = new AuditManager();
                var lstResults = am.RetrieveByDateRange(from, to);
                return Ok(new { data = new { items = lstResults } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Filtro por módulo/entidad afectada (pantalla Engineer/Audit)
        [HttpGet]
        [Route("ByModule")]
        public ActionResult ByModule([FromQuery] string module, [FromQuery] string callerRole = "")
        {
            try
            {
                var am = new AuditManager();
                var lstResults = am.RetrieveByModule(module);
                return Ok(new { data = new { items = lstResults } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Filtro por usuario responsable (pantalla Engineer/Audit)
        [HttpGet]
        [Route("ByUser")]
        public ActionResult ByUser([FromQuery] int userId)
        {
            try
            {
                var am = new AuditManager();
                var lstResults = am.RetrieveByUser(userId);
                return Ok(new { data = new { items = lstResults } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("RetrieveById/{id}")]
        public ActionResult RetrieveById(int id)
        {
            try
            {
                var am = new AuditManager();
                var audit = am.RetrieveById(id);
                return Ok(audit);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Audit audit)
        {
            try
            {
                var am = new AuditManager();
                am.Create(audit);
                return Ok(audit);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
