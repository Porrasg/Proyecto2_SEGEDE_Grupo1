using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        // Cuerpo simple para pedir la exportación desde el frontend sin meter clases extras.
        public class ExportStatementRequest
        {
            public int StatementId { get; set; }
            public string Format { get; set; } = string.Empty;
        }

        // Trae los estados de cuenta reales desde la base y, si llega buyerId, los filtra por ese comprador.
        [HttpGet]
        [Route("Statements")]
        public ActionResult Statements([FromQuery] int? buyerId)
        {
            try
            {
                var im = new InvoiceManager();
                var invoices = im.RetrieveAllInvoices();

                if (buyerId.HasValue && buyerId.Value > 0)
                {
                    invoices = invoices.Where(i => i.BuyerId == buyerId.Value).ToList();
                }

                return Ok(new { data = invoices });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Exporta el estado de cuenta como CSV, XLSX o PDF real y registra la descarga.
        [HttpPost]
        [Route("Export")]
        public IActionResult Export(ExportStatementRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                var invoice = new InvoiceManager().RetrieveById(request.StatementId);
                var actorUserId = AuditHelper.ResolveCallerUserId(callerUserId);
                var isBuyer = User.IsInRole("Buyer") || User.IsInRole("Customer") || User.IsInRole("Distributor");
                if (isBuyer && actorUserId != invoice.BuyerId)
                {
                    return Forbid();
                }

                var exportRequest = new FileExportRequest
                {
                    Title = $"Estado de Cuenta {invoice.InvoiceNumber}",
                    FileName = $"EstadoCuenta_{invoice.Id}",
                    Format = request.Format,
                    Headers = new List<string> { "Campo", "Valor" },
                    Rows = new List<List<string?>>
                    {
                        new() { "Factura", invoice.Id.ToString() },
                        new() { "Número", invoice.InvoiceNumber },
                        new() { "Comprador", invoice.BuyerId.ToString() },
                        new() { "Fecha emisión", invoice.IssueDate.ToString("yyyy-MM-dd") },
                        new() { "Fecha vencimiento", invoice.DueDate.ToString("yyyy-MM-dd") },
                        new() { "Energía (MWh)", invoice.EnergyMWh.ToString("0.00") },
                        new() { "Precio unitario", invoice.UnitPrice.ToString("0.00") },
                        new() { "Subtotal", invoice.Subtotal.ToString("0.00") },
                        new() { "Impuesto %", invoice.TaxPercentage.ToString("0.00") },
                        new() { "Impuesto", invoice.TaxAmount.ToString("0.00") },
                        new() { "Total", invoice.TotalAmount.ToString("0.00") },
                        new() { "Estado", invoice.PaymentStatus }
                    }
                };

                var result = new FileExportManager().Generate(exportRequest);
                AuditHelper.TryAudit(actorUserId, "Download", "FileExports", invoice.Id,
                    $"Estado de cuenta {invoice.InvoiceNumber} descargado como {result.FileName}");
                return File(result.Content, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

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

        // Permite marcar una factura como pagada desde la interfaz.
        [HttpPatch]
        [Route("MarkAsPaid/{id}")]
        public ActionResult MarkAsPaid(int id, [FromQuery] int? callerUserId)
        {
            try
            {
                var im = new InvoiceManager();
                var invoice = im.RetrieveById(id);
                invoice.PaymentStatus = "Paid";
                im.Update(invoice);
                AuditHelper.TryAudit(callerUserId, "Update", "Invoices", invoice.Id, "Factura marcada como pagada");
                return Ok(new { message = "Factura marcada como pagada.", data = invoice });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
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
