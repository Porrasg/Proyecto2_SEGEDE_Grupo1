using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    public class BaseDTO
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

    }
}
