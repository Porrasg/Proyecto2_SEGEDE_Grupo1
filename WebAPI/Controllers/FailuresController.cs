using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FailuresController : ControllerBase
    {
        // Cuerpo del reporte de falla que envía el frontend
        public class FailureRegisterRequest
        {
            public int TurbineId { get; set; }
            public string Severity { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        [HttpGet]
        [Route("RetrieveAll")]
        [Route("All")] // alias que usan las pantallas de operaciones y Reportes
        public ActionResult RetrieveAll()
        {
            try
            {
                var fm = new FailureManager();
                var lstResults = fm.RetrieveAllFailures();
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(FailuresController), ex);
            }
        }

        // Reporta una falla nueva. El ingeniero que reporta es el usuario autenticado
        // (callerUserId); el manager valida su rol/estado y asigna el estado inicial
        // "Reported". Si la severidad es Critical, la turbina pasa a Damaged y el
        // cambio queda en la bitácora de auditoría.
        [HttpPost]
        [Route("Register")]
        public ActionResult Register(FailureRegisterRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                if (!actorUserId.HasValue)
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario que reporta la falla." });
                }

                Turbine? affectedTurbine = null;
                if (request.Severity == "Critical")
                {
                    affectedTurbine = new TurbineManager().RetrieveTurbineById(request.TurbineId);
                    var canBecomeDamaged = affectedTurbine.Status == "Damaged" ||
                        (TurbineManager.AllowedTransitions.TryGetValue(affectedTurbine.Status, out var allowed) &&
                         allowed.Contains("Damaged", StringComparer.Ordinal));
                    if (!canBecomeDamaged)
                    {
                        return BadRequest(new
                        {
                            message = $"La turbina en estado {affectedTurbine.Status} no admite registrar una falla crítica."
                        });
                    }
                }

                var fm = new FailureManager();
                var failure = new Failure
                {
                    TurbineId = request.TurbineId,
                    EngineerId = actorUserId.Value,
                    Severity = request.Severity,
                    Description = request.Description,
                    FailureDate = DateTime.Now
                };

                fm.Create(failure);

                AuditHelper.TryAudit(actorUserId, "Create", "Failures", failure.Id, $"Falla reportada en turbina #{request.TurbineId} (severidad: {request.Severity})");

                // Lógica cruzada: una falla crítica deja la turbina fuera de operación
                if (request.Severity == "Critical")
                {
                    var tm = new TurbineManager();
                    if (affectedTurbine!.Status != "Damaged")
                    {
                        tm.ChangeState(request.TurbineId, "Damaged");
                        AuditHelper.TryAudit(actorUserId, "ChangeState", "Turbines", request.TurbineId,
                            $"Estado: {affectedTurbine.Status} -> Damaged. Motivo: falla crítica #{failure.Id}: {request.Description}");
                    }
                }

                return Ok(new { message = request.Severity == "Critical"
                    ? "Falla crítica reportada. La turbina pasó a estado Damaged."
                    : "Falla reportada con éxito." });
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(FailuresController), ex);
            }
        }

        [HttpGet]
        [Route("RetrieveById/{id}")]
        public ActionResult RetrieveById(int id)
        {
            try
            {
                var fm = new FailureManager();
                var failure = fm.RetrieveById(id);
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(FailuresController), ex);
            }
        }

        [HttpGet]
        [Route("RetrieveByTurbineId/{turbineId}")]
        [Route("ByTurbine/{turbineId}")] // alias que usa la pantalla de operaciones
        public ActionResult RetrieveByTurbineId(int turbineId)
        {
            try
            {
                var fm = new FailureManager();
                var lstResults = fm.RetrieveByTurbineId(turbineId);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(FailuresController), ex);
            }
        }

        [HttpGet]
        [Route("RetrieveByEngineerId/{engineerId}")]
        public ActionResult RetrieveByEngineerId(int engineerId)
        {
            try
            {
                var fm = new FailureManager();
                var lstResults = fm.RetrieveByEngineerId(engineerId);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(FailuresController), ex);
            }
        }

        [HttpGet]
        [Route("RetrieveByStatus/{status}")]
        public ActionResult RetrieveByStatus(string status)
        {
            try
            {
                var fm = new FailureManager();
                var lstResults = fm.RetrieveByStatus(status);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(FailuresController), ex);
            }
        }

        [HttpGet]
        [Route("RetrieveBySeverity/{severity}")]
        public ActionResult RetrieveBySeverity(string severity)
        {
            try
            {
                var fm = new FailureManager();
                var lstResults = fm.RetrieveBySeverity(severity);
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(FailuresController), ex);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Failure failure, [FromQuery] int? callerUserId)
        {
            try
            {
                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                if (!actorUserId.HasValue)
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario que reporta la falla." });
                }

                failure.EngineerId = actorUserId.Value;
                failure.FailureDate = DateTime.Now;
                var fm = new FailureManager();
                fm.Create(failure);
                AuditHelper.TryAudit(actorUserId, "Create", "Failures", failure.Id,
                    $"Falla reportada en turbina #{failure.TurbineId} (endpoint directo)");
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(FailuresController), ex);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Failure failure, [FromQuery] int? callerUserId)
        {
            try
            {
                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                if (!actorUserId.HasValue)
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario que actualiza la falla." });
                }

                var fm = new FailureManager();
                var previousStatus = fm.RetrieveById(failure.Id).Status;
                fm.Update(failure);
                AuditHelper.TryAudit(actorUserId, "Update", "Failures", failure.Id,
                    previousStatus == failure.Status
                        ? "Información técnica de la falla actualizada"
                        : $"Estado: {previousStatus} -> {failure.Status}. Información técnica de la falla actualizada");
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(FailuresController), ex);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Failure failure, [FromQuery] int? callerUserId)
        {
            try
            {
                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                if (!actorUserId.HasValue)
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario que cancela la falla." });
                }

                var fm = new FailureManager();
                fm.Delete(failure);
                AuditHelper.TryAudit(actorUserId, "Cancel", "Failures", failure.Id, "Falla cancelada");
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(FailuresController), ex);
            }
        }
    }
}
