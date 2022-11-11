using System;
using System.Collections.Generic;

#nullable disable

namespace FP.Models
{
    public partial class Location
    {
        public Location()
        {
            Orders = new HashSet<Order>();
        }

        public decimal Id { get; set; }
        public decimal? UserId { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public string PostaZip { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
