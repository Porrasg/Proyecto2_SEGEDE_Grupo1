using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Representa un registro de auditoría de acciones realizadas en el sistema
    // Almacena el historial de acciones realizadas por los usuarios
    public class Audit : BaseDTO
    {
        public int? UserId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty; // indica la entidad o módulo afectado

        public int? EntityId { get; set; } // identificador del registro afectado

        public string Description { get; set; } = string.Empty;

        public string? IpAddress { get; set; } // dirección IP desde donde se realizó la operación
    }
}
