using System;
using System.Collections.Generic;

#nullable disable

namespace FP.Models
{
    public partial class Feedback
    {
        public decimal Id { get; set; }
        public decimal? UserId { get; set; }
        public string Body { get; set; }

        public virtual User User { get; set; }
    }
}
