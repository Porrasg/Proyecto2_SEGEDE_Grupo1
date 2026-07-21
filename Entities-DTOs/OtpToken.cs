using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    public class OtpToken : BaseDTO
    {
        public string Email { get; set; }
        public string TokenCode { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsUsed { get; set; }
    }

}

