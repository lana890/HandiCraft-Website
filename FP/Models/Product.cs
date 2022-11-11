using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace FP.Models
{
    public partial class Product
    {
        public Product()
        {
            Carts = new HashSet<Cart>();
            Orders = new HashSet<Order>();
        }

        public decimal Id { get; set; }
        public string Name { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Price { get; set; }
        public string ImagePath { get; set; }
        public decimal? CartagoryId { get; set; }
        public decimal? VendorId { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }

        public virtual Catagory Cartagory { get; set; }
        public virtual User Vendor { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
