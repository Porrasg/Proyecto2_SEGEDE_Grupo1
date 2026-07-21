using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Representa un usuario registrado en el sistema
    public class User : BaseDTO
    {
        public string Identification { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string FirstLastName { get; set; } = string.Empty;

        public string? SecondLastName { get; set; }

        public DateTime BirthDate { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? ProfilePhoto { get; set; }

        public string Password { get; set; } = string.Empty;

        public int Age { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int FailedLoginAttempts { get; set; } // Número de intentos fallidos de inicio de sesión.

        public DateTime? LockoutEndAt { get; set; } // Fecha y hora hasta la que la cuenta permanecerá bloqueada por intentos fallidos.

        public DateTime? LastLoginAt { get; set; } // Fecha y hora del último inicio de sesión exitoso.
    }
}
