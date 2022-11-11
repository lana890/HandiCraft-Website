using System;
using System.Collections.Generic;

#nullable disable

namespace FP.Models
{
    public partial class Invoice
    {
        public decimal Id { get; set; }
        public decimal? UserId { get; set; }
        public DateTime? Datee { get; set; }
        public string Massege { get; set; }

        public virtual User User { get; set; }
    }
}
