using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Representa un vaciado o transferencia de energía almacenada desde una batería hacia el banco central de energía
    public class Flush : BaseDTO
    {
        public int FlushBatchId { get; set; }

        public int TurbineId { get; set; }

        public int BatteryId { get; set; }

        public int CentralBankId { get; set; }

        public decimal SnapshotEnergyMWh { get; set; } // guarda la energía que tenía la batería justo antes del vaciado

        public decimal TransferredEnergyMWh { get; set; } //almacena la cantidad de energía que realmente se transfirió al banco central

        public decimal SaturationLossMWh { get; set; } // registra la energía que no pudo ingresar al banco central por falta de capacidad disponible

        public string ExecutionType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime ExecutedAt { get; set; }
    }
}
