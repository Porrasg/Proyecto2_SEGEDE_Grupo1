using System;

namespace Entities_DTOs
{
    // Representa el precio vigente (o histórico) de la energía por MWh
    public class Price : BaseDTO
    {
        public decimal PriceCRCPerMWh { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public bool IsActive { get; set; }
    }
}
