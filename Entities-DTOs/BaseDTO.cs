using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    public class BaseDTO
    {
        // Propiedad Id que representa el identificador único de la entidad
        // en la BD cada uno se llama asi UserId, TurbineId, MaintenanceId, FailureId, BatteryId, FlushId, CentralBankId, ForecastId, DistributionId, InvoiceId, NotificationId, AuditId
        // pero para simplificar el código se usa solo Id
        public int Id { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
