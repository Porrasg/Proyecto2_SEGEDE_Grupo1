using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    public class BatterySnapshot : BaseDTO
    {
        public int FlushId { get; set; }
        public int BatteryId { get; set; }
        public int TurbineId { get; set; }
        public decimal MaximumCapacityMWh { get; set; }
        public decimal CurrentEnergyMWh { get; set; }
        public decimal TotalGeneratedMWh { get; set; }
        public decimal TotalTransferredMWh { get; set; }
        public decimal TotalSaturationLossMWh { get; set; }
        public string Status { get; set; }
        public DateTime CapturedAt { get; set; }
    }
}