using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    public class CentralBankMovement : BaseDTO
    {
        public int CentralBankId { get; set; }
        public string MovementType { get; set; } = string.Empty; // tipo de movimiento: Ingreso, Egreso o flush cuando viene de la bateria 
        public decimal EnergyMWh { get; set; }
        public decimal InventoryAfterMovement { get; set; }
        public decimal SaturationLossMWh { get; set; } // cantidad de energia que se pierde por saturacion de la bateria, si es que la hay
        public string Description { get; set; } = string.Empty;

    }
}
