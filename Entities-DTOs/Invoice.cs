using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Almacena las facturas generadas a partir de las distribuciones de energía realizadas por el sistema
    public class Invoice : BaseDTO
    {
        public string InvoiceNumber { get; set; } = string.Empty;

        public int DistributionId { get; set; }

        public int BuyerId { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime DueDate { get; set; }

        public decimal EnergyMWh { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TaxPercentage { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public string PaymentStatus { get; set; } = string.Empty;
    }
}
