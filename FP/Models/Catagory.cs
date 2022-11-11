using System;
using System.Collections.Generic;

#nullable disable

namespace FP.Models
{
    public partial class Catagory
    {
        public Catagory()
        {
            Products = new HashSet<Product>();
        }

        public decimal Id { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
