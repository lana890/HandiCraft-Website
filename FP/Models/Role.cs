using System;
using System.Collections.Generic;

#nullable disable

namespace FP.Models
{
    public partial class Role
    {
        public Role()
        {
            Logins = new HashSet<Login>();
        }

        public decimal Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Login> Logins { get; set; }
    }
}
