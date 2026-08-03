using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    public class EnergyGeneration : BaseDTO
    {

        public int TurbineId { get; set; }
        public decimal GenerateMWh { get; set; }
        public decimal WindSpeedMs { get; set; }
        public DateTime GenerateAt { get; set; }
        public DateTime CreateAt { get; set; }

    }
}
