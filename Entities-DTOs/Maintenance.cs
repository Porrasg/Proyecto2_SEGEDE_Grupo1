using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Representa un mantenimiento de una turbina
    public class Maintenance : BaseDTO
    {
        public int TurbineId { get; set; }

        public int EngineerId { get; set; }

        public string MaintenanceType { get; set; } = string.Empty; // maneja valores Preventive, Corrective, Predictive, Inspection

        public string Description { get; set; } = string.Empty;

        public DateTime EstimatedStartDate { get; set; }

        public DateTime EstimatedEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        public string? Result { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
