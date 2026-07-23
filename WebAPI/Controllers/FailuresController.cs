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
                return StatusCode(500, ex.Message);
            }
        }

        // Reporta una falla nueva. El ingeniero que reporta es el usuario autenticado
        // (callerUserId); el manager valida su rol/estado y asigna el estado inicial
        // "Reported". Si la severidad es Critical, la turbina pasa a Damaged y el
        // cambio queda en la bitácora de auditoría.
        [HttpPost]
        [Route("Register")]
        public ActionResult Register(FailureRegisterRequest request, [FromQuery] int callerUserId)
        {
            try
            {
                var fm = new FailureManager();
                var failure = new Failure
                {
                    TurbineId = request.TurbineId,
                    EngineerId = callerUserId,
                    Severity = request.Severity,
                    Description = request.Description,
                    FailureDate = DateTime.Now
                };

                fm.Create(failure);

                // Lógica cruzada: una falla crítica deja la turbina fuera de operación
                if (request.Severity == "Critical")
                {
                    var tm = new TurbineManager();
                    tm.ChangeState(request.TurbineId, "Damaged");

                    try
                    {
                        var am = new AuditManager();
                        am.Create(new Audit
                        {
                            UserId = callerUserId,
                            Action = "Update",
                            EntityName = "Turbines",
                            EntityId = request.TurbineId,
                            Description = $"Turbina marcada como Damaged por falla crítica: {request.Description}"
                        });
                    }
                    catch
                    {
                        // La auditoría no debe impedir el reporte ya registrado
                    }
                }

                return Ok(new { message = request.Severity == "Critical"
                    ? "Falla crítica reportada. La turbina pasó a estado Damaged."
                    : "Falla reportada con éxito." });
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
                var fm = new FailureManager();
                var failure = fm.RetrieveById(id);
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
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
                return StatusCode(500, ex.Message);
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
                return StatusCode(500, ex.Message);
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
                return StatusCode(500, ex.Message);
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
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Failure failure)
        {
            try
            {
                var fm = new FailureManager();
                fm.Create(failure);
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Failure failure)
        {
            try
            {
                var fm = new FailureManager();
                fm.Update(failure);
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Failure failure)
        {
            try
            {
                var fm = new FailureManager();
                fm.Delete(failure);
                return Ok(failure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
