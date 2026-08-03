using CoreApp;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        public class SetPermissionRequest
        {
            public string RoleName { get; set; } = string.Empty;
            public string ModuleKey { get; set; } = string.Empty;
            public string ModuleLabel { get; set; } = string.Empty;
            public bool CanAccess { get; set; }
        }

        [HttpGet]
        [Route("Permissions")]
        public ActionResult Permissions()
        {
            try
            {
                var rm = new RoleManager();
                var lstResults = rm.RetrieveAllPermissions();
                return Ok(new { data = lstResults });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("SetPermission")]
        public ActionResult SetPermission(SetPermissionRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                var rm = new RoleManager();
                rm.SetPermission(request.RoleName, request.ModuleKey, request.ModuleLabel, request.CanAccess);

                AuditHelper.TryAudit(callerUserId, "Update", "RolePermissions", null,
                    $"Permiso '{request.ModuleKey}' de {request.RoleName} → {(request.CanAccess ? "habilitado" : "deshabilitado")}");

                return Ok(new { message = "Permiso actualizado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
