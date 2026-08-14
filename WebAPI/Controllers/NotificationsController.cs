using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var nm = new NotificationManager();
                var lstResults = nm.RetrieveAllNotifications();
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Notification notification, [FromQuery] int? callerUserId)
        {
            try
            {
                var nm = new NotificationManager();
                nm.Create(notification);
                AuditHelper.TryAudit(callerUserId, "Create", "Notifications", notification.Id, "Notificación creada");
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Notification notification, [FromQuery] int? callerUserId)
        {
            try
            {
                var nm = new NotificationManager();
                nm.Update(notification);
                AuditHelper.TryAudit(callerUserId, "Update", "Notifications", notification.Id, "Notificación actualizada");
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Notification notification, [FromQuery] int? callerUserId)
        {
            try
            {
                var nm = new NotificationManager();
                nm.Delete(notification);
                AuditHelper.TryAudit(callerUserId, "LogicalDelete", "Notifications", notification.Id, "Notificación eliminada");
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ProcessQueue")]
        public ActionResult ProcessQueue([FromQuery] int? callerUserId)
        {
            try
            {
                var processed = new NotificationManager().ProcessOverdueInvoices();
                AuditHelper.TryAudit(callerUserId, "Process", "Notifications", null,
                    $"Cola procesada: {processed} factura(s) vencida(s)");
                return Ok(new
                {
                    message = processed == 0
                        ? "No había facturas vencidas pendientes de notificación."
                        : $"Se procesaron {processed} factura(s) vencida(s).",
                    data = new { processed }
                });
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(NotificationsController), ex);
            }
        }
    }
}
