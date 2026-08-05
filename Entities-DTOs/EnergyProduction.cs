using System;

namespace Entities_DTOs
{
    // Registro de producción neta calculada para una turbina en un periodo
    // (Operación y Cortes para Almacenamiento)
    public class EnergyProduction : BaseDTO
    {
        public int TurbineId { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime EventDate { get; set; } // fecha del corte (fin del periodo)

        public decimal GrossEnergyMWh { get; set; } // producción bruta según capacidad nominal

        public decimal MaintenanceLossMWh { get; set; } // pérdida por mantenimiento/inactividad

        public decimal GeneratedEnergy { get; set; } // producción neta = bruta - pérdida
    }
}
