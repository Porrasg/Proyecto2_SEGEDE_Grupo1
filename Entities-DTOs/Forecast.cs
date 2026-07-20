using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Representa una solicitud mensual de energía
    public class Forecast : BaseDTO
    {
        public int BuyerId { get; set; }

        public int ForecastYear { get; set; }

        public int ForecastMonth { get; set; }

        public decimal RequestedEnergyMWh { get; set; } // cantidad de energía que el comprador solicita

        public string Status { get; set; } = string.Empty;
    }
}
