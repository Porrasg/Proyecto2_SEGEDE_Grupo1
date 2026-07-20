using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Representa una falla reportada en una turbina
    public class Failure : BaseDTO
    {
        public int TurbineId { get; set; }

        public int EngineerId { get; set; }

        public DateTime FailureDate { get; set; }

        public string Severity { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty; // explica qué falla ocurrió

        public string? Resolution { get; set; } // explica cómo se solucionó

        public string Status { get; set; } = string.Empty;
    }
}
