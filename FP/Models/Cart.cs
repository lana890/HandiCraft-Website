using System;
using System.Collections.Generic;

#nullable disable

namespace FP.Models
{
    public partial class Cart
    {
        public decimal Id { get; set; }
        public decimal? ProductId { get; set; }
        public decimal? UserId { get; set; }
        public decimal? Amount { get; set; }

        public virtual Product Product { get; set; }
        public virtual User User { get; set; }
    }
}
