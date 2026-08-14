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
        // Cuerpo simple para exportar un estado de cuenta sin meter librerías extra.
        public class ExportStatementRequest
        {
            public int StatementId { get; set; }
            public string Format { get; set; } = string.Empty;
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
    }
}
