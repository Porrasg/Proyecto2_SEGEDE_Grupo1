using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
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

        // Exportación sencilla: CSV para descargar y HTML básico para simular el PDF.
        [HttpPost]
        [Route("Export")]
        public IActionResult Export(ExportStatementRequest request)
        {
            try
            {
                var invoice = new InvoiceManager().RetrieveById(request.StatementId);
                var format = (request.Format ?? "CSV").Trim().ToUpperInvariant();
                var fileNameBase = $"EstadoCuenta_{invoice.Id}";

                if (format == "EXCEL")
                {
                    var csv = BuildStatementCsv(invoice);
                    var bom = new UTF8Encoding(true).GetPreamble();
                    var csvBytes = Encoding.UTF8.GetBytes(csv);
                    var result = bom.Concat(csvBytes).ToArray();
                    return File(result, "text/csv; charset=utf-8", fileNameBase + ".csv");
                }

                if (format == "PDF")
                {
                    var html = BuildStatementHtml(invoice);
                    return File(Encoding.UTF8.GetBytes(html), "text/html; charset=utf-8", fileNameBase + ".html");
                }

                var csvDefault = BuildStatementCsv(invoice);
                var bomDefault = new UTF8Encoding(true).GetPreamble();
                var csvBytesDefault = Encoding.UTF8.GetBytes(csvDefault);
                return File(bomDefault.Concat(csvBytesDefault).ToArray(), "text/csv; charset=utf-8", fileNameBase + ".csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Arma un CSV legible para abrirlo ordenado en Excel.
        private static string BuildStatementCsv(Invoice invoice)
        {
            // CSV limpio para abrirlo bonito en Excel.
            var sb = new StringBuilder();
            sb.AppendLine("Campo,Valor");
            sb.AppendLine("Factura," + CsvCell(invoice.Id));
            sb.AppendLine("Número," + CsvCell(invoice.InvoiceNumber));
            sb.AppendLine("Comprador," + CsvCell(invoice.BuyerId));
            sb.AppendLine("Fecha emisión," + CsvCell(invoice.IssueDate.ToString("yyyy-MM-dd")));
            sb.AppendLine("Fecha vencimiento," + CsvCell(invoice.DueDate.ToString("yyyy-MM-dd")));
            sb.AppendLine("Energía (MWh)," + CsvCell(invoice.EnergyMWh.ToString("N2")));
            sb.AppendLine("Precio unitario," + CsvCell(invoice.UnitPrice.ToString("N2")));
            sb.AppendLine("Subtotal," + CsvCell(invoice.Subtotal.ToString("N2")));
            sb.AppendLine("Impuesto %," + CsvCell(invoice.TaxPercentage.ToString("N2")));
            sb.AppendLine("Impuesto," + CsvCell(invoice.TaxAmount.ToString("N2")));
            sb.AppendLine("Total," + CsvCell(invoice.TotalAmount.ToString("N2")));
            sb.AppendLine("Estado," + CsvCell(invoice.PaymentStatus));
            return sb.ToString();
        }

        // Escapa una celda para que comas y comillas no rompan el archivo.
        private static string CsvCell(object? value)
        {
            var text = value?.ToString() ?? string.Empty;
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }

        // Genera una vista HTML simple para que el usuario la pueda imprimir o guardar.
        private static string BuildStatementHtml(Invoice invoice)
        {
            // HTML básico para que el comprador pueda imprimir o guardar la vista.
            return $@"<!doctype html>
<html lang='es'>
<head>
    <meta charset='utf-8'>
    <title>Estado de Cuenta {HtmlValue(invoice.Id.ToString())}</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 24px; color: #222; }}
        table {{ border-collapse: collapse; width: 100%; max-width: 800px; }}
        th, td {{ border: 1px solid #ccc; padding: 8px 10px; text-align: left; }}
        th {{ background: #f2f2f2; }}
    </style>
</head>
<body>
    <h2>Estado de Cuenta #{HtmlValue(invoice.Id.ToString())}</h2>
    <table>
        <tr><th>Número</th><td>{HtmlValue(invoice.InvoiceNumber)}</td></tr>
        <tr><th>Comprador</th><td>{HtmlValue(invoice.BuyerId.ToString())}</td></tr>
        <tr><th>Fecha emisión</th><td>{invoice.IssueDate:yyyy-MM-dd}</td></tr>
        <tr><th>Fecha vencimiento</th><td>{invoice.DueDate:yyyy-MM-dd}</td></tr>
        <tr><th>Energía (MWh)</th><td>{invoice.EnergyMWh:N2}</td></tr>
        <tr><th>Precio unitario</th><td>{invoice.UnitPrice:N2}</td></tr>
        <tr><th>Subtotal</th><td>{invoice.Subtotal:N2}</td></tr>
        <tr><th>Impuesto %</th><td>{invoice.TaxPercentage:N2}</td></tr>
        <tr><th>Impuesto</th><td>{invoice.TaxAmount:N2}</td></tr>
        <tr><th>Total</th><td>{invoice.TotalAmount:N2}</td></tr>
        <tr><th>Estado</th><td>{HtmlValue(invoice.PaymentStatus)}</td></tr>
    </table>
</body>
</html>";
        }

        // Evita que texto con caracteres raros rompa el HTML.
        private static string HtmlValue(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
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
                return StatusCode(500, ex.Message);
            }
        }
    }
}
