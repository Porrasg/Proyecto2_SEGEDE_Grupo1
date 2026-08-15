using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingController : ControllerBase
    {
        // Cuerpos de las acciones administrativas sobre estados de cuenta.
        public class AnnulStatementRequest
        {
            public int StatementId { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        public class RegenerateStatementRequest
        {
            public int OriginalStatementId { get; set; }
        }

        public class SetPriceRequest
        {
            public decimal PriceCRCPerMWh { get; set; }
        }

        public class SetTaxRequest
        {
            public string Name { get; set; } = string.Empty;
            public decimal Percentage { get; set; }
        }

        [HttpPost]
        [Route("SetPrice")]
        public ActionResult SetPrice(SetPriceRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                var bm = new BillingManager();
                bm.SetPrice(request.PriceCRCPerMWh);

                AuditHelper.TryAudit(callerUserId, "Create", "Prices", null,
                    $"Nuevo precio vigente: {request.PriceCRCPerMWh} CRC/MWh");

                return Ok(new { message = "Precio registrado con éxito." });
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(BillingController), ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("PriceHistory")]
        public ActionResult PriceHistory()
        {
            try
            {
                var bm = new BillingManager();
                var lstResults = bm.RetrievePriceHistory();
                return Ok(new { data = lstResults });
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(BillingController), ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ActivePrice")]
        public ActionResult ActivePrice()
        {
            try
            {
                var bm = new BillingManager();
                var price = bm.RetrieveActivePrice();
                return Ok(new { data = price });
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(BillingController), ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("SetTax")]
        public ActionResult SetTax(SetTaxRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                var bm = new BillingManager();
                bm.SetTax(request.Name, request.Percentage);

                AuditHelper.TryAudit(callerUserId, "Create", "Taxes", null,
                    $"Nuevo impuesto vigente: {request.Name} ({request.Percentage:P2})");

                return Ok(new { message = "Impuesto registrado con éxito." });
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(BillingController), ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("TaxHistory")]
        public ActionResult TaxHistory()
        {
            try
            {
                var bm = new BillingManager();
                var lstResults = bm.RetrieveTaxHistory();
                return Ok(new { data = lstResults });
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(BillingController), ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ActiveTax")]
        public ActionResult ActiveTax()
        {
            try
            {
                var bm = new BillingManager();
                var tax = bm.RetrieveActiveTax();
                return Ok(new { data = tax });
            }
            catch (Exception ex)
            {
                AppLogger.LogError(nameof(BillingController), ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("AnnulStatement")]
        public ActionResult AnnulStatement(AnnulStatementRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest(new { message = "El motivo de anulación es obligatorio." });
                }

                var manager = new InvoiceManager();
                var invoice = manager.RetrieveById(request.StatementId);
                if (invoice.PaymentStatus == "Paid")
                {
                    return BadRequest(new { message = "No se puede anular un estado de cuenta pagado." });
                }
                if (invoice.PaymentStatus == "Cancelled")
                {
                    return BadRequest(new { message = "El estado de cuenta ya está anulado." });
                }
                manager.Delete(invoice);
                AuditHelper.TryAudit(callerUserId, "Cancel", "Invoices", invoice.Id,
                    $"Estado de cuenta {invoice.InvoiceNumber} anulado. Motivo: {request.Reason.Trim()}");
                return Ok(new { message = "Estado de cuenta anulado correctamente." });
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(BillingController), ex);
            }
        }

        [HttpPost]
        [Route("RegenerateStatement")]
        public ActionResult RegenerateStatement(RegenerateStatementRequest request, [FromQuery] int? callerUserId)
        {
            try
            {
                var manager = new InvoiceManager();
                var original = manager.RetrieveById(request.OriginalStatementId);
                if (original.PaymentStatus != "Cancelled")
                {
                    return BadRequest(new { message = "Solo se puede regenerar un estado de cuenta anulado." });
                }

                var regenerated = new Invoice
                {
                    DistributionId = original.DistributionId,
                    BuyerId = original.BuyerId,
                    TaxPercentage = original.TaxPercentage
                };
                manager.Create(regenerated);
                AuditHelper.TryAudit(callerUserId, "Regenerate", "Invoices", regenerated.Id,
                    $"Estado de cuenta regenerado a partir de {original.InvoiceNumber}");
                return Ok(new
                {
                    message = "Estado de cuenta regenerado correctamente.",
                    data = regenerated
                });
            }
            catch (Exception ex)
            {
                return ApiErrorHelper.Handle(nameof(BillingController), ex);
            }
        }
    }
}
