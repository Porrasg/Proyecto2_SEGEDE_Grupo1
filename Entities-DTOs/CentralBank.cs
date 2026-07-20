using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Representa el banco central de energía
    public class CentralBank : BaseDTO
    {
        public string Name { get; set; } = string.Empty;

        public decimal MaximumCapacityMWh { get; set; } //cantidad máxima de energía que puede almacenar el banco central

        public decimal CurrentInventoryMWh { get; set; }

        public decimal TotalReceivedMWh { get; set; } // no representa el inventario actual, sino un total histórico

        public decimal TotalDistributedMWh { get; set; } // acumula toda la energía que ya fue distribuida entre los compradores

        public decimal TotalSaturationLossMWh { get; set; } // energía que no pudo ingresar al banco central porque este alcanzó su capacidad máxima

        public string Status { get; set; } = string.Empty;
    }
}
