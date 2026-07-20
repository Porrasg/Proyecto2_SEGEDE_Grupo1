using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Representa una notificación para un usuario del sistema
    public class Notification : BaseDTO
    {
        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string NotificationType { get; set; } = string.Empty;

        public string? ReferenceType { get; set; }

        public int? ReferenceId { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }
    }
}
