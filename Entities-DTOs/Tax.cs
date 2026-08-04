using System;

namespace Entities_DTOs
{
    // Representa el porcentaje de impuesto vigente (o histórico) aplicado a la facturación
    public class Tax : BaseDTO
    {
        public string Name { get; set; } = string.Empty;

        public decimal Percentage { get; set; } // fracción, ej. 0.13 para 13%

        public DateTime ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public bool IsActive { get; set; }
    }
}
