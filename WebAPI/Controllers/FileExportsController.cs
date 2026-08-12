using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileExportsController : ControllerBase
    {
        [HttpPost]
        [Route("Download")]
        public IActionResult Download(FileExportRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                var result = new FileExportManager().Generate(request);
                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                AuditHelper.TryAudit(actorUserId, "Download", "FileExports", null,
                    $"Archivo {result.FileName} descargado en formato {result.Format}; {result.RowCount} fila(s)");

                return File(result.Content, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("History")]
        public IActionResult History([FromQuery] int limit = 100)
        {
            try
            {
                limit = Math.Clamp(limit, 1, 500);
                var entries = new AuditManager().RetrieveByModule("FileExports")
                    .Where(entry => string.Equals(entry.Action, "Download", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(entry => entry.CreatedAt)
                    .Take(limit)
                    .ToList();
                return Ok(new { data = entries });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
