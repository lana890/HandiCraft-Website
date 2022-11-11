using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace FP.Models
{
    public partial class User
    {
        public User()
        {
            Carts = new HashSet<Cart>();
            Contacts = new HashSet<Contact>();
            Feedbacks = new HashSet<Feedback>();
            Invoices = new HashSet<Invoice>();
            Locations = new HashSet<Location>();
            Logins = new HashSet<Login>();
            OrderUsers = new HashSet<Order>();
            OrderVendors = new HashSet<Order>();
            Products = new HashSet<Product>();
        }

        public decimal Id { get; set; }
        public decimal? Age { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Phonenumber { get; set; }
        public string ImagePath { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        [NotMapped]
        public bool isVendor { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Contact> Contacts { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
        public virtual ICollection<Location> Locations { get; set; }
        public virtual ICollection<Login> Logins { get; set; }
        public virtual ICollection<Order> OrderUsers { get; set; }
        public virtual ICollection<Order> OrderVendors { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
