using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Representa una batería asociada a una turbina
    public class Battery : BaseDTO
    {
        public int TurbineId { get; set; }

        public decimal MaximumCapacityMWh { get; set; } // Capacidad máxima de la batería en MWh

        public decimal CurrentEnergyMWh { get; set; } // Energía actual almacenada en la batería en MWh

        public decimal TotalGeneratedMWh { get; set; } // Conserva la energía total que la turbina ha enviado a la batería durante las simulaciones

        public decimal TotalTransferredMWh { get; set; } // Registra la energía acumulada que ha sido vaciada desde la batería hacia el banco central

        public decimal TotalSaturationLossMWh { get; set; } // Guarda la energía que no pudo almacenarse porque la batería alcanzó su capacidad máxima

        public string Status { get; set; } = string.Empty;
    }
}
