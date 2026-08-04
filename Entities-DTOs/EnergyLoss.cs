using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    public class EnergyLoss : BaseDTO
    {
        public int TurbineId { get; set; }
        public int? BatteryId { get; set; } // Puede ser nulo porque no siempre hay una batería asociada a la pérdida de energía
        public decimal LostMWh { get; set; }
        public string Reason { get; set; }
        public DateTime OccurredAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
