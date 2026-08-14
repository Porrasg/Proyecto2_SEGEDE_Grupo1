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
                var actorUserId = AuditHelper.ResolveCallerUserId(callerUserId);
                if (!actorUserId.HasValue)
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario que descarga el archivo." });
                }

                var result = new FileExportManager().Generate(request);
                AuditHelper.TryAudit(actorUserId, "Download", "FileExports", null,
                    $"Archivo {result.FileName} descargado en formato {result.Format}; {result.RowCount} fila(s)");

                return File(result.Content, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(FileExportsController), ex);
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
                return ApiErrorHelper.Handle(nameof(FileExportsController), ex);
            }
        }
    }
}
