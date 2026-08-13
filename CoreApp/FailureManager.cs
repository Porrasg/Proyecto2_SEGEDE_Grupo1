using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class FailureManager
    {
        private static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            { "Reported", new[] { "InProgress", "Resolved", "Cancelled" } },
            { "InProgress", new[] { "Resolved", "Cancelled" } },
            { "Resolved", Array.Empty<string>() },
            { "Cancelled", Array.Empty<string>() }
        };

        public List<Failure> RetrieveAllFailures()
        {
            var crud = new FailureCrudFactory();
            return crud.RetrieveAll<Failure>();
        }
        public Failure RetrieveById(int id)
        {
            // Validar el identificador de la falla
            if (id <= 0)
            {
                throw new BusinessException("El identificador de la falla no es válido");
            }

            var crud = new FailureCrudFactory();

            var failure = crud.RetrieveById<Failure>(id);

            // Validar que la falla exista
            if (failure == null)
            {
                throw new BusinessException( "No se encontró la falla solicitada");
            }

            return failure;
        }
        public void Create(Failure failure)
        {
            if (failure == null)
            {
                throw new BusinessException("La falla no puede ser nula");
            }

            // Asignar el estado inicial de la falla
            failure.Status = "Reported";

            if (HasEmptyFields(failure))
            {
                throw new BusinessException("Todos los campos obligatorios deben completarse");
            }

            // Validar la severidad de la falla
            if (!IsValidSeverity(failure.Severity))
            {
                throw new BusinessException("La severidad de la falla no es válida");
            }

            // Validar el estado de la falla
            if (!IsValidStatus(failure.Status))
            {
                throw new BusinessException("El estado de la falla no es válido");
            }
            
            if (failure.FailureDate == default(DateTime))
            {
                throw new BusinessException("La fecha de la falla es obligatoria");
            }
            
            if (failure.FailureDate > DateTime.Now)
            {
                throw new BusinessException(
                    "La fecha de la falla no puede ser futura");
            }

            var turbineCrud = new TurbineCrudFactory();

            // Obtener la turbina asociada a la falla
            var turbine = turbineCrud.RetrieveById<Turbine>(failure.TurbineId);

            if (turbine == null)
            {
                throw new BusinessException("La turbina seleccionada no existe");
            }

            // Validar que la turbina no esté dada de baja
            if (turbine.Status == "Decommissioned")
            {
                throw new BusinessException("No se puede reportar una falla para una turbina dada de baja");
            }

            var userCrud = new UserCrudFactory();

            // Obtener el ingeniero asociado a la falla
            var engineer = userCrud.RetrieveById<User>(failure.EngineerId);

            if (engineer == null)
            {
                throw new BusinessException("El ingeniero seleccionado no existe");
            }

            // Validar que el usuario tenga el rol de ingeniero
            if (engineer.Role != "Engineer")
            {
                throw new BusinessException("El usuario seleccionado no tiene el rol de ingeniero");
            }

            // Validar que el ingeniero se encuentre activo
            if (engineer.Status != "Active")
            {
                throw new BusinessException("El ingeniero seleccionado no se encuentra activo");
            }

            // Asignar los valores iniciales de la falla
            failure.Resolution = null;
            failure.CreatedAt = DateTime.Now;
            failure.UpdatedAt = null;

            var crud = new FailureCrudFactory();

            crud.Create(failure);

            // Alarma de criticidad sistémica: notificar al ingeniero asignado cuando la falla es
            // Critical. Cada canal en su propio try/catch para no revertir la falla ya registrada.
            if (failure.Severity == "Critical")
            {
                try
                {
                    new OtpManager().SendGenericEmail(
                        engineer.Email,
                        engineer.FirstName,
                        "ALERTA CRÍTICA - Falla en Turbina #" + failure.TurbineId,
                        $"Se reportó una falla de severidad <strong>Crítica</strong> en la turbina #{failure.TurbineId}. " +
                        $"Descripción: {failure.Description}");
                }
                catch { /* no bloquear la falla ya registrada */ }

                try
                {
                    new NotificationManager().Create(new Notification
                    {
                        UserId = engineer.Id,
                        Title = "Alarma crítica de falla",
                        Message = $"Turbina #{failure.TurbineId}: {failure.Description}",
                        NotificationType = "Failure"
                    });
                }
                catch { /* no bloquear la falla ya registrada */ }
            }
        }
        public void Update(Failure failure)
        {
            if (failure == null)
            {
                throw new BusinessException("La falla no puede ser nula");
            }

            if (failure.Id <= 0)
            {
                throw new BusinessException("El identificador de la falla no es válido");
            }

            if (HasEmptyFields(failure))
            {
                throw new BusinessException("Todos los campos obligatorios deben completarse");
            }

            // Validar la severidad de la falla
            if (!IsValidSeverity(failure.Severity))
            {
                throw new BusinessException("La severidad de la falla no es válida");
            }

            // Validar el estado de la falla
            if (!IsValidStatus(failure.Status))
            {
                throw new BusinessException("El estado de la falla no es válido");
            }

            // Validar la fecha de la falla
            if (failure.FailureDate == default(DateTime))
            {
                throw new BusinessException("La fecha de la falla es obligatoria");
            }

            // Validar que la fecha de la falla no sea futura
            if (failure.FailureDate > DateTime.Now)
            {
                throw new BusinessException("La fecha de la falla no puede ser futura");
            }

            // Validar la resolución de una falla resuelta
            if (failure.Status == "Resolved" &&
                string.IsNullOrWhiteSpace(failure.Resolution))
            {
                throw new BusinessException("Una falla resuelta debe incluir una descripción de la solución");
            }

            var failureCrud = new FailureCrudFactory();

            // Obtener la falla registrada
            var currentFailure = failureCrud.RetrieveById<Failure>(failure.Id);

            if (currentFailure == null)
            {
                throw new BusinessException("La falla que desea actualizar no existe");
            }

            // Una falla finalizada es inmutable y no puede reabrirse silenciosamente.
            if (currentFailure.Status == "Cancelled" || currentFailure.Status == "Resolved")
            {
                throw new BusinessException("No se puede modificar una falla resuelta o cancelada");
            }

            if (failure.TurbineId != currentFailure.TurbineId ||
                failure.EngineerId != currentFailure.EngineerId ||
                failure.FailureDate != currentFailure.FailureDate)
            {
                throw new BusinessException("La turbina, el ingeniero y la fecha original de una falla no se pueden modificar");
            }

            if (!string.Equals(failure.Status, currentFailure.Status, StringComparison.Ordinal) &&
                (!AllowedTransitions.TryGetValue(currentFailure.Status, out var allowed) ||
                 !allowed.Contains(failure.Status, StringComparer.Ordinal)))
            {
                throw new BusinessException($"La transición de {currentFailure.Status} a {failure.Status} no está permitida");
            }

            var turbineCrud = new TurbineCrudFactory();

            // Obtener la turbina asociada a la falla
            var turbine = turbineCrud.RetrieveById<Turbine>(failure.TurbineId);

            if (turbine == null)
            {
                throw new BusinessException("La turbina seleccionada no existe");
            }

            // Validar que la turbina no esté dada de baja
            if (turbine.Status == "Decommissioned")
            {
                throw new BusinessException("No se puede asignar la falla a una turbina dada de baja");
            }

            var userCrud = new UserCrudFactory();

            // Obtener el ingeniero asociado a la falla
            var engineer = userCrud.RetrieveById<User>(failure.EngineerId);

            if (engineer == null)
            {
                throw new BusinessException("El ingeniero seleccionado no existe");
            }

            if (engineer.Role != "Engineer")
            {
                throw new BusinessException("El usuario seleccionado no tiene el rol de ingeniero");
            }

            if (engineer.Status != "Active")
            {
                throw new BusinessException("El ingeniero seleccionado no se encuentra activo");
            }

            // Mantener la fecha original de creación
            failure.CreatedAt = currentFailure.CreatedAt;

            // Asignar la fecha de actualización
            failure.UpdatedAt = DateTime.Now;

            // Actualizar la falla
            failureCrud.Update(failure);
        }
        public void Delete(Failure failure)
        {
            if (failure == null)
            {
                throw new BusinessException("La falla no puede ser nula");
            }

            if (failure.Id <= 0)
            {
                throw new BusinessException("El identificador de la falla no es válido");
            }

            var failureCrud = new FailureCrudFactory();

            // Obtener la falla registrada
            var currentFailure =failureCrud.RetrieveById<Failure>(failure.Id);

            if (currentFailure == null)
            {
                throw new BusinessException("La falla que desea cancelar no existe");
            }

            // Validar que la falla no esté cancelada
            if (currentFailure.Status == "Cancelled")
            {
                throw new BusinessException("La falla ya se encuentra cancelada");
            }

            // Validar que la falla no esté resuelta
            if (currentFailure.Status == "Resolved")
            {
                throw new BusinessException("No se puede cancelar una falla resuelta");
            }

            // La cancelación lógica siempre utiliza la fila persistida, no los datos
            // opcionales recibidos desde el cliente.
            currentFailure.Status = "Cancelled";

            // Asignar la fecha de actualización
            currentFailure.UpdatedAt = DateTime.Now;

            // Cancelar lógicamente la falla
            failureCrud.Delete(currentFailure);
        }

        public List<Failure> RetrieveByTurbineId(int turbineId)
        {
            if (turbineId <= 0)
            {
                throw new BusinessException("El identificador de la turbina no es válido");
            }

            var turbineCrud = new TurbineCrudFactory();

            // Obtener la turbina registrada
            var turbine = turbineCrud.RetrieveById<Turbine>(turbineId);

            // Validar que la turbina exista
            if (turbine == null)
            {
                throw new BusinessException("La turbina seleccionada no existe");
            }

            var failureCrud = new FailureCrudFactory();

            // Obtener las fallas asociadas a la turbina
            return failureCrud.RetrieveByTurbineId(turbineId);
        }


        public List<Failure> RetrieveByEngineerId(int engineerId)
        {
            if (engineerId <= 0)
            {
                throw new BusinessException("El identificador del ingeniero no es válido");
            }

            var userCrud = new UserCrudFactory();

            // Obtener el ingeniero registrado
            var engineer = userCrud.RetrieveById<User>(engineerId);

            // Validar que el ingeniero exista
            if (engineer == null)
            {
                throw new BusinessException("El ingeniero seleccionado no existe");
            }

            // Validar que el usuario tenga el rol de ingeniero
            if (engineer.Role != "Engineer")
            {
                throw new BusinessException("El usuario seleccionado no tiene el rol de ingeniero");
            }

            var failureCrud = new FailureCrudFactory();

            // Obtener las fallas asociadas al ingeniero
            return failureCrud.RetrieveByEngineerId(engineerId);
        }


        public List<Failure> RetrieveByStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                throw new BusinessException("El estado de la falla es obligatorio");
            }
            
            if (!IsValidStatus(status))
            {
                throw new BusinessException("El estado de la falla no es válido");
            }

            var crud = new FailureCrudFactory();

            // Obtener las fallas asociadas al estado
            return crud.RetrieveByStatus(status);
        }


        public List<Failure> RetrieveBySeverity(string severity)
        {
            // Validar que la severidad haya sido ingresada
            if (string.IsNullOrWhiteSpace(severity))
            {
                throw new BusinessException("La severidad de la falla es obligatoria");
            }

            // Validar la severidad de la falla
            if (!IsValidSeverity(severity))
            {
                throw new BusinessException("La severidad de la falla no es válida");
            }

            var failureCrud = new FailureCrudFactory();

            // Obtener las fallas asociadas a la severidad
            return failureCrud.RetrieveBySeverity(severity);
        }


        private bool HasEmptyFields(Failure failure)
        {
            return failure.TurbineId <= 0 ||
                   failure.EngineerId <= 0 ||
                   string.IsNullOrWhiteSpace(failure.Severity) ||
                   string.IsNullOrWhiteSpace(failure.Description) ||
                   string.IsNullOrWhiteSpace(failure.Status);
        }

        private bool IsValidSeverity(string severity)
        {
            return severity == "Low" ||
                   severity == "Medium" ||
                   severity == "High" ||
                   severity == "Critical";
        }

        private bool IsValidStatus(string status)
        {
            return status == "Reported" ||
                   status == "InProgress" ||
                   status == "Resolved" ||
                   status == "Cancelled";
        }

    }
}
