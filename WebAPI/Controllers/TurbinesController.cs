using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurbinesController : ControllerBase
    {
        // Cuerpo del registro de turbinas que envía el frontend (sin estado:
        // toda turbina nueva inicia como "Active").
        public class TurbineRegisterRequest
        {
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string Brand { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public int ManufactureYear { get; set; }
            public decimal NominalWeeklyCapacityMWh { get; set; }
        }

        // Cuerpo del cambio de estado operativo que envía el frontend
        public class ChangeStateRequest
        {
            public int TurbineId { get; set; }
            public string NewState { get; set; } = string.Empty;
            public string? Reason { get; set; }
        }

        private static readonly Regex StateChangePattern = new(
            @"^Estado:\s*(?<previous>[A-Za-z]+)\s*->\s*(?<next>[A-Za-z]+)\.\s*Motivo:\s*(?<reason>.*)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        [HttpGet]
        [Route("RetrieveAll")]
        [Route("All")] // alias que usa el módulo de Reportes
        public ActionResult RetrieveAll()
        {
            try
            {
                var tm = new TurbineManager();
                var lstResults = tm.RetrieveAllTurbines();
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(TurbinesController), ex);
            }
        }

        [HttpGet]
        [Route("Statuses")]
        public ActionResult Statuses()
        {
            // Fuente única de los estados operativos válidos: el diccionario vive en
            // TurbineManager, no hardcodeado aquí ni en el frontend.
            var statuses = TurbineManager.ValidStatuses
                .Select(s => new { value = s.Key, label = s.Value });
            return Ok(statuses);
        }

        [HttpGet]
        [Route("AllowedTransitions/{id}")]
        public ActionResult AllowedTransitions(int id)
        {
            try
            {
                var tm = new TurbineManager();
                var statuses = tm.GetAllowedTransitions(id)
                    .Select(value => new { value, label = TurbineManager.ValidStatuses[value] });
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(TurbinesController), ex);
            }
        }

        [HttpGet]
        [Route("RetrieveById/{id}")]
        public ActionResult RetrieveById(int id)
        {
            try
            {
                var tm = new TurbineManager();
                var turbine = tm.RetrieveTurbineById(id);
                return Ok(turbine);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(TurbinesController), ex);
            }
        }

        [HttpPost]
        [Route("Register")]
        public ActionResult Register(TurbineRegisterRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                if (!actorUserId.HasValue)
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario que registra la turbina." });
                }

                var tm = new TurbineManager();
                var turbine = new Turbine
                {
                    Code = request.Code,
                    Name = request.Name,
                    Location = request.Location,
                    Brand = request.Brand,
                    Model = request.Model,
                    ManufactureYear = request.ManufactureYear,
                    NominalWeeklyCapacityMWh = request.NominalWeeklyCapacityMWh,
                    Status = "Active"
                };

                tm.Create(turbine);

                //Registrar la creacion de la turbina en la bitacora 
                AuditHelper.TryAudit(actorUserId, "Create", "Turbines", turbine.Id, $"Turbina {turbine.Code} registrada con estado Active");


                return Ok(new { message = "Turbina registrada con éxito.", data = turbine });
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(TurbinesController), ex);
            }
        }

        [HttpPost]
        [Route("ChangeState")]
        public ActionResult ChangeState(ChangeStateRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest(new { message = "El motivo técnico del cambio de estado es obligatorio." });
                }

                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                if (!actorUserId.HasValue)
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario que cambia el estado." });
                }

                var tm = new TurbineManager();
                var previousState = tm.RetrieveTurbineById(request.TurbineId).Status;
                tm.ChangeState(request.TurbineId, request.NewState);

                //Registrar el cambio de estado de la turbina
                AuditHelper.TryAudit(actorUserId, "ChangeState", "Turbines", request.TurbineId,
                    $"Estado: {previousState} -> {request.NewState}. Motivo: {request.Reason.Trim()}");

                return Ok(new { message = "Estado de la turbina actualizado." });
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(TurbinesController), ex);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Turbine turbine, [FromQuery] int? callerUserId)
        {
            try
            {
                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                if (!actorUserId.HasValue)
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario que registra la turbina." });
                }

                turbine.Status = "Active";
                var tm = new TurbineManager();
                tm.Create(turbine);
                AuditHelper.TryAudit(actorUserId, "Create", "Turbines", turbine.Id,
                    $"Turbina {turbine.Code} registrada con estado Active (endpoint directo)");
                return Ok(turbine);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(TurbinesController), ex);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Turbine turbine, [FromQuery] int? callerUserId)
        {
            try
            {
                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                if (!actorUserId.HasValue)
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario que actualiza la turbina." });
                }

                var tm = new TurbineManager();
                tm.Update(turbine);

                //Registrar la actualizacion de la turbina en la bitacora
                AuditHelper.TryAudit(actorUserId, "Update", "Turbines", turbine.Id, $"Información de la turbina {turbine.Code} actualizada");

                return Ok(turbine);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(TurbinesController), ex);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Turbine turbine, [FromQuery] string? reason, [FromQuery] int? callerUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    return BadRequest(new { message = "El motivo técnico de la baja es obligatorio." });
                }

                var actorUserId = AuditHelper.ResolveCallerUserId(User, callerUserId);
                if (!actorUserId.HasValue)
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario que da de baja la turbina." });
                }

                var tm = new TurbineManager();
                var previousState = tm.RetrieveTurbineById(turbine.Id).Status;
                tm.Delete(turbine);
                AuditHelper.TryAudit(actorUserId, "ChangeState", "Turbines", turbine.Id,
                    $"Estado: {previousState} -> Decommissioned. Motivo: {reason.Trim()}");
                return Ok(turbine);
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(TurbinesController), ex);
            }
        }
       
        // =========================================================================
        // 🔹 ENDPOINTS PARA DETALLE TÉCNICO Y MÉTRICAS / HISTORIAL DE TURBINAS
        // =========================================================================

        [HttpGet]
        [Route("Metrics/{id}")]
        public ActionResult GetMetrics(int id, [FromQuery] int periodDays = 30)
        {
            try
            {
                return Ok(new TurbineManager().RetrieveOperationalMetrics(id, periodDays));
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(TurbinesController), ex);
            }
        }

        [HttpGet]
        [Route("History/{id}")]
        public ActionResult GetHistory(int id)
        {
            try
            {
                var audits = new AuditManager().RetrieveAllAudits() ?? new List<Audit>();
                var logs = audits
                    .Where(a => a != null &&
                                string.Equals(a.EntityName, "Turbines", StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(a.Action, "ChangeState", StringComparison.OrdinalIgnoreCase) &&
                                a.EntityId == id)
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a =>
                    {
                        var match = StateChangePattern.Match(a.Description ?? string.Empty);
                        return new
                        {
                            auditId = a.Id,
                            date = a.CreatedAt,
                            previousState = match.Success ? match.Groups["previous"].Value : string.Empty,
                            newState = match.Success ? match.Groups["next"].Value : string.Empty,
                            reason = match.Success ? match.Groups["reason"].Value : a.Description,
                            userId = a.UserId
                        };
                    });
                return Ok(logs);
            }
            catch (Exception ex) { return ApiErrorHelper.Handle(nameof(TurbinesController), ex); }
        }

        [HttpGet]
        [Route("Maintenances/{id}")]
        public ActionResult GetMaintenances(int id)
        {
            try
            {
                return Ok(new MaintenanceManager().RetrieveByTurbineId(id));
            }
            catch (Exception ex) { return ApiErrorHelper.Handle(nameof(TurbinesController), ex); }
        }

        [HttpGet]
        [Route("Failures/{id}")]
        public ActionResult GetFailures(int id)
        {
            try
            {
                return Ok(new FailureManager().RetrieveByTurbineId(id));
            }
            catch (Exception ex) { return ApiErrorHelper.Handle(nameof(TurbinesController), ex); }
        }
    }
}
