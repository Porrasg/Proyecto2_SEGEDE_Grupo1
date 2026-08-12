using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var im = new InvoiceManager();
                var lstResults = im.RetrieveAllInvoices();
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(InvoicesController), ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveById/{id}")]
        public ActionResult RetrieveById(int id)
        {
            try
            {
                var im = new InvoiceManager();
                var invoice = im.RetrieveById(id);
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(InvoicesController), ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(Invoice invoice, [FromQuery] int? callerUserId)
        {
            try
            {
                var im = new InvoiceManager();
                im.Create(invoice);
                AuditHelper.TryAudit(callerUserId, "Create", "Invoices", invoice.Id, "Factura creada");
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(InvoicesController), ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(Invoice invoice, [FromQuery] int? callerUserId)
        {
            try
            {
                var im = new InvoiceManager();
                im.Update(invoice);
                AuditHelper.TryAudit(callerUserId, "Update", "Invoices", invoice.Id, "Factura actualizada");
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(InvoicesController), ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(Invoice invoice, [FromQuery] int? callerUserId)
        {
            try
            {
                var im = new InvoiceManager();
                im.Delete(invoice);
                AuditHelper.TryAudit(callerUserId, "LogicalDelete", "Invoices", invoice.Id, "Factura anulada");
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(InvoicesController), ex);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
