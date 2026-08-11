using CoreApp;
using Entities_DTOs;

namespace WebAPI
{
    // Envuelve el registro de auditoría en el mismo patrón defensivo ya usado en
    // varios controladores: un fallo al auditar nunca debe impedir ni revertir
    // una operación de negocio que ya se aplicó con éxito.
    public static class AuditHelper
    {
        public static void TryAudit(int? callerUserId, string action, string entityName, int? entityId, string description)
        {
            try
            {

                // Los usuarios estáticos del sistema usan IDs negativos. Como no existen en tblUsers, no se registran como UserId real.
                int? validUserId =
                    callerUserId.HasValue && callerUserId.Value > 0
                        ? callerUserId
                        : null;

                new AuditManager().Create(new Audit
                {
                    UserId = callerUserId,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    Description = description
                });
            }
            catch
            {
                // No bloquear la operación ya aplicada por un fallo de auditoría
            }
        }
    }
}
