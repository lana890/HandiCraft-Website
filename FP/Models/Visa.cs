using System;
using System.Collections.Generic;

#nullable disable

namespace FP.Models
{
    public partial class Visa
    {
        public decimal Id { get; set; }
        public string SerNumber { get; set; }
        public string Exp { get; set; }
        public string Cvv { get; set; }
        public decimal? Balance { get; set; }
    }
}
