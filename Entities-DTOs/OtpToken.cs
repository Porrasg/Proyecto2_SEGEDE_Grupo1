using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    public class OtpToken : BaseDTO
    {
        public string Email { get; set; }
        public string TokenCode { get; set; }
        public string Purpose { get; set; } // Indica para qué operación se generó el OTP
        public DateTime ExpirationDate { get; set; }
        public bool IsUsed { get; set; }
    }

}

