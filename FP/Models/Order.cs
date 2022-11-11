using System;
using System.Collections.Generic;

#nullable disable

namespace FP.Models
{
    public partial class Order
    {
        public decimal Id { get; set; }
        public decimal? ProductId { get; set; }
        public DateTime? Datee { get; set; }
        public decimal? Amount { get; set; }
        public decimal? UserId { get; set; }
        public decimal? VendorId { get; set; }
        public decimal? LocationId { get; set; }
        public decimal? Paid { get; set; }
        public string Note { get; set; }

        public virtual Location Location { get; set; }
        public virtual Product Product { get; set; }
        public virtual User User { get; set; }
        public virtual User Vendor { get; set; }
    }
}
