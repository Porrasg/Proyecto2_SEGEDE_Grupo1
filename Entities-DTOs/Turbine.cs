using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Representa una turbina de generación eléctrica
    public class Turbine : BaseDTO
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int ManufactureYear { get; set; }

        public decimal NominalWeeklyCapacityMWh { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
