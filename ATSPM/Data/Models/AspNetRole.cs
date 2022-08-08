using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class AspNetRole
    {
        public AspNetRole()
        {
            Users = new HashSet<AspNetUser>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Discriminator { get; set; } = null!;

        public virtual ICollection<AspNetUser> Users { get; set; }
    }
}
