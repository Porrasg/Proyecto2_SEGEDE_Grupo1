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

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int FailedLoginAttempts { get; set; }

        public DateTime? LockoutEndAt { get; set; }

        public DateTime? LastLoginAt { get; set; }
    }
}
