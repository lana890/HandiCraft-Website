using System;
using System.Collections.Generic;

#nullable disable

namespace FP.Models
{
    public partial class Contact
    {
        public decimal Id { get; set; }
        public string Subject { get; set; }
        public string Massege { get; set; }
        public decimal? UserId { get; set; }

        public virtual User User { get; set; }
    }
}
