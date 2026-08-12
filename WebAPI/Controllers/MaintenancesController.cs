using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenancesController : ControllerBase
    {
        // Cuerpo de la programación de mantenimiento que envía la pantalla de operaciones
        public class ScheduleRequest
        {
            public int TurbineId { get; set; }
            public string MaintenanceType { get; set; } = string.Empty;
            public DateTime EstimatedStartDate { get; set; }
            public DateTime EstimatedEndDate { get; set; }
        }

        // Cuerpo para marcar un mantenimiento como completado
        public class CompleteRequest
        {
            public int MaintenanceId { get; set; }
            public string Result { get; set; } = string.Empty;
        }

        [HttpGet]
        [Route("RetrieveAll")]
        [Route("All")] // alias que usan las pantallas de operaciones y Reportes
        public ActionResult RetrieveAll()
        {
            try
            {
                var mm = new MaintenanceManager();
                var lstResults = mm.RetrieveAllMaintenances();
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Turbinas sin ningún mantenimiento agendado en el mes en curso (obligatoriedad mensual)
        [HttpGet]
        [Route("ComplianceAlert")]
        public ActionResult ComplianceAlert()
        {
            try
            {
                var mm = new MaintenanceManager();
                var lstResults = mm.RetrieveTurbinesWithoutMonthlyMaintenance();
                return Ok(new { data = lstResults });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Mantenimientos de una turbina específica (filtro de la pantalla de operaciones)
        [HttpGet]
        [Route("ByTurbine/{turbineId}")]
        public ActionResult ByTurbine(int turbineId)
        {
            try
            {
                var mm = new MaintenanceManager();
                var lstResults = mm.RetrieveByTurbineId(turbineId);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Programa un mantenimiento nuevo. El ingeniero responsable es el usuario
        // autenticado (callerUserId); el manager valida su rol y estado, la regla
        // de una sola turbina en mantenimiento por día y las fechas estimadas.
        [HttpPost]
        [Route("Schedule")]
        public ActionResult Schedule(ScheduleRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                if (!actorUserId.HasValue)
                {
                    return Unauthorized(new { message = "No se pudo identificar al ingeniero responsable." });
                }

                var mm = new MaintenanceManager();
                var maintenance = new Maintenance
                {
                    TurbineId = request.TurbineId,
                    EngineerId = actorUserId.Value,
                    MaintenanceType = request.MaintenanceType,
                    Description = $"Mantenimiento {request.MaintenanceType} programado desde el sistema",
                    EstimatedStartDate = request.EstimatedStartDate,
                    EstimatedEndDate = request.EstimatedEndDate
                };

                mm.Create(maintenance);
                AuditHelper.TryAudit(actorUserId, "Create", "Maintenances", maintenance.Id, $"Mantenimiento {request.MaintenanceType} programado para turbina #{request.TurbineId}");
                return Ok(new { message = "Mantenimiento programado con éxito.", data = maintenance });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Marca un mantenimiento como completado con su informe de resultado y,
        // si la turbina quedó en estado Maintenance, la reactiva.
        [HttpPost]
        [Route("Complete")]
        public ActionResult Complete(CompleteRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                var mm = new MaintenanceManager();
                var maintenance = mm.RetrieveById(request.MaintenanceId);

                // Fechas reales: si nunca se registró el inicio real, se usa la fecha
                // estimada (o la de creación si la estimada aún no llega) para que
                // el manager acepte inicio < fin.
                if (!maintenance.ActualStartDate.HasValue)
                {
                    maintenance.ActualStartDate = maintenance.EstimatedStartDate <= DateTime.Now
                        ? maintenance.EstimatedStartDate
                        : maintenance.CreatedAt;
                }

                maintenance.ActualEndDate = DateTime.Now;
                maintenance.Result = request.Result;
                maintenance.Status = "Completed";

                mm.Update(maintenance);
                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                AuditHelper.TryAudit(actorUserId, "Update", "Maintenances", maintenance.Id, "Mantenimiento marcado como completado");

                // Lógica cruzada: al completar el mantenimiento la turbina vuelve a operar
                var tm = new TurbineManager();
                var turbine = tm.RetrieveTurbineById(maintenance.TurbineId);
                if (turbine.Status == "Maintenance")
                {
                    tm.ChangeState(turbine.Id, "Active");
                    AuditHelper.TryAudit(actorUserId, "ChangeState", "Turbines", turbine.Id,
                        $"Estado: Maintenance -> Active. Motivo: mantenimiento #{maintenance.Id} completado");
                }

                return Ok(new { message = "Mantenimiento completado con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Cancela un mantenimiento programado (baja lógica vía el manager)
        [HttpPost]
        [Route("Cancel/{id}")]
        public ActionResult Cancel(int id, [FromQuery] int? callerUserId)
        {
            try
            {
                var mm = new MaintenanceManager();
                var maintenance = mm.RetrieveById(id);

                mm.Delete(maintenance);
                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                AuditHelper.TryAudit(actorUserId, "Cancel", "Maintenances", id, "Mantenimiento cancelado.");
                return Ok(new { message = "Mantenimiento cancelado." });
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
                var mm = new MaintenanceManager();
                var maintenance = mm.RetrieveById(id);
                return Ok(maintenance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Maintenance maintenance, [FromQuery] int? callerUserId)
        {
            try
            {
                var mm = new MaintenanceManager();
                mm.Create(maintenance);
                AuditHelper.TryAudit(callerUserId, "Create", "Maintenances", maintenance.Id, "Mantenimiento creado (endpoint directo)");
                return Ok(maintenance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Maintenance maintenance, [FromQuery] int? callerUserId)
        {
            try
            {
                var mm = new MaintenanceManager();
                mm.Update(maintenance);
                AuditHelper.TryAudit(callerUserId, "Update", "Maintenances", maintenance.Id, "Mantenimiento actualizado (endpoint directo)");
                return Ok(maintenance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Maintenance maintenance, [FromQuery] int? callerUserId)
        {
            try
            {
                var mm = new MaintenanceManager();
                mm.Delete(maintenance);
                AuditHelper.TryAudit(callerUserId, "Delete", "Maintenances", maintenance.Id, "Mantenimiento eliminado (endpoint directo)");
                return Ok(maintenance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
