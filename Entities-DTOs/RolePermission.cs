using System;

namespace Entities_DTOs
{
    // Indica si un rol tiene acceso a un módulo del sistema (matriz de permisos configurable)
    public class RolePermission : BaseDTO
    {
        public string RoleName { get; set; } = string.Empty;

        public string ModuleKey { get; set; } = string.Empty;

        public string ModuleLabel { get; set; } = string.Empty;

        public bool CanAccess { get; set; }
    }
}
