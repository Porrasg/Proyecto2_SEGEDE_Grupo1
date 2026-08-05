using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
                return StatusCode(500, ex.Message);
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
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Register")]
        public ActionResult Register(TurbineRegisterRequest request)
        {
            try
            {
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
                return Ok(new { message = "Turbina registrada con éxito.", data = turbine });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ChangeState")]
        public ActionResult ChangeState(ChangeStateRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                var tm = new TurbineManager();
                tm.ChangeState(request.TurbineId, request.NewState);

                // El motivo del cambio queda en la bitácora de auditoría (la tabla
                // de turbinas no guarda un historial de estados propio).
                try
                {
                    var am = new AuditManager();
                    am.Create(new Audit
                    {
                        UserId = callerUserId,
                        Action = "Update",
                        EntityName = "Turbines",
                        EntityId = request.TurbineId,
                        Description = $"Cambio de estado a {request.NewState}. Motivo: {request.Reason ?? "No indicado"}"
                    });
                }
                catch
                {
                    // La auditoría no debe impedir el cambio de estado ya aplicado
                }

                return Ok(new { message = "Estado de la turbina actualizado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Turbine turbine)
        {
            try
            {
                var tm = new TurbineManager();
                tm.Create(turbine);
                return Ok(turbine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Turbine turbine)
        {
            try
            {
                var tm = new TurbineManager();
                tm.Update(turbine);
                return Ok(turbine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Turbine turbine)
        {
            try
            {
                var tm = new TurbineManager();
                tm.Delete(turbine);
                return Ok(turbine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
       
        // =========================================================================
        // 🔹 ENDPOINTS PARA DETALLE TÉCNICO Y MÉTRICAS / HISTORIAL DE TURBINAS
        // =========================================================================

        [HttpGet]
        [Route("Metrics/{id}")]
        public ActionResult GetMetrics(int id)
        {
            try
            {
                var turbine = new TurbineManager().RetrieveTurbineById(id);
                if (turbine == null)
                    return NotFound(new { message = $"No se encontró la turbina con ID {id}" });

                var maints = new MaintenanceManager().RetrieveByTurbineId(id) ?? new List<Maintenance>();

                // Enviamos SOLO los datos crudos
                return Ok(new
                {
                    availability = turbine.ManufactureYear,
                    maintenances = maints
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("History/{id}")]
        public ActionResult GetHistory(int id)
        {
            try
            {
                var audits = new AuditManager().RetrieveAllAudits() ?? new List<Audit>();
                var logs = audits.Where(a => a != null && string.Equals(a.EntityName, "Turbines", StringComparison.OrdinalIgnoreCase) && a.EntityId == id);
                return Ok(logs);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet]
        [Route("Maintenances/{id}")]
        public ActionResult GetMaintenances(int id)
        {
            try
            {
                var maints = new MaintenanceManager().RetrieveAllMaintenances() ?? new List<Maintenance>();
                return Ok(maints.Where(m => m != null && m.TurbineId == id));
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet]
        [Route("Failures/{id}")]
        public ActionResult GetFailures(int id)
        {
            try
            {
                var failures = new FailureManager().RetrieveAllFailures() ?? new List<Failure>();
                return Ok(failures.Where(f => f != null && f.TurbineId == id));
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}
    }
}
