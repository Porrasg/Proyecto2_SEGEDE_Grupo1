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

        // Ejecuta un vaciado manual: recorre las baterías activas con energía
        // disponible y crea un Flush por cada una dentro del mismo lote. El
        // cálculo de transferencia/saturación y la actualización de la batería
        // y del Banco Central los hace el SP transaccional CRE_FLUSH_PR, que ya
        // invoca FlushManager.Create — aquí solo se orquesta el recorrido.
        [HttpPost]
        [Route("ExecuteManual")]
        public ActionResult ExecuteManual([FromQuery] int? callerUserId)
        {
            try
            {
                var cbManager = new CentralBankManager();
                var centralBanks = cbManager.RetrieveAllCentralBanks();

                if (centralBanks.Count == 0)
                {
                    return StatusCode(500, new { message = "No hay un Banco Central registrado para recibir la energía." });
                }

                var centralBank = centralBanks[0];

                var batteryManager = new BatteryManager();
                var batteries = batteryManager.RetrieveAllBatteries();

                var flushManager = new FlushManager();

                // El lote agrupa todos los vaciados de esta ejecución
                int batchId = 1;
                foreach (var previous in flushManager.RetrieveAllFlushes())
                {
                    if (previous.FlushBatchId >= batchId)
                    {
                        batchId = previous.FlushBatchId + 1;
                    }
                }

                int processed = 0;

                foreach (var battery in batteries)
                {
                    // Solo baterías activas con energía disponible
                    if (battery.Status != "Active" || battery.CurrentEnergyMWh <= 0)
                    {
                        continue;
                    }

                    flushManager.Create(new Flush
                    {
                        FlushBatchId = batchId,
                        TurbineId = battery.TurbineId,
                        BatteryId = battery.Id,
                        CentralBankId = centralBank.Id,
                        ExecutionType = "Manual",
                        ExecutedAt = DateTime.Now
                    });

                    processed++;
                }

                if (processed == 0)
                {
                    return Ok(new { message = "No hay baterías activas con energía disponible para vaciar." });
                }

                // Constancia de la ejecución en la bitácora de auditoría
                try
                {
                    var am = new AuditManager();
                    am.Create(new Audit
                    {
                        UserId = callerUserId,
                        Action = "Execute",
                        EntityName = "Flushes",
                        Description = $"Vaciado manual del lote {batchId}: {processed} batería(s) trasladada(s) al Banco Central"
                    });
                }
                catch
                {
                    // La auditoría no debe anular el vaciado ya ejecutado
                }

                return Ok(new { message = $"Vaciado manual ejecutado: {processed} batería(s) procesada(s) en el lote {batchId}." });
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
