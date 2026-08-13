using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    public class FlushConfig : BaseDTO
    {
        public TimeSpan ExecutionTime { get; set; }
        public bool IsAutomatic { get; set; }
    }
}
