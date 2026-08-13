using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlushsController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var fm = new FlushManager();
                var lstResults = fm.RetrieveAllFlushes();
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Ejecuta un vaciado manual masivo de todas las baterías activas
        // que tengan energía disponible. La lógica del proceso se encuentra en FlushManager.ExecuteMassFlush()
        [HttpPost]
        [Route("ExecuteManual")]
        public ActionResult ExecuteManual([FromQuery] int? callerUserId)
        {
            try
            {
                var flushManager = new FlushManager();

                int processed =
                    flushManager.ExecuteMassFlush("Manual");

                if (processed == 0)
                {
                    return Ok(new
                    {
                        message =
                            "No hay baterías activas con energía disponible para vaciar."
                    });
                }

                // Registrar la ejecución en auditoría
                try
                {
                    var am = new AuditManager();

                    am.Create(new Audit
                    {
                        UserId = callerUserId,
                        Action = "Execute",
                        EntityName = "Flushes",
                        Description =
                            $"Vaciado manual masivo: {processed} batería(s) trasladada(s) al Banco Central"
                    });
                }
                catch
                {
                    // La auditoría no debe cancelar el vaciado
                }

                return Ok(new
                {
                    message =
                        $"Vaciado manual ejecutado: {processed} batería(s) procesada(s)."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("RetrieveById/{id}")]
        public ActionResult RetrieveById(int id)
        {
            try
            {
                var fm = new FlushManager();
                var flush = fm.RetrieveById(id);
                return Ok(flush);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Flush flush)
        {
            try
            {
                var fm = new FlushManager();
                fm.Create(flush);
                return Ok(flush);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Flush flush)
        {
            try
            {
                var fm = new FlushManager();
                fm.Update(flush);
                return Ok(flush);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Flush flush)
        {
            try
            {
                var fm = new FlushManager();
                fm.Delete(flush);
                return Ok(flush);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveConfiguration")]
        public ActionResult RetrieveConfiguration()
        {
            try
            {
                var fm = new FlushConfigManager();
                var config = fm.RetrieveConfiguration();

                return Ok(config);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateConfiguration")]
        public ActionResult UpdateConfiguration(FlushConfig config)
        {
            try
            {
                var fm = new FlushConfigManager();
                fm.UpdateConfiguration(config);

                return Ok(config);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
