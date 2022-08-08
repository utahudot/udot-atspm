using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class Agency
    {
        public Agency()
        {
            ActionLogs = new HashSet<ActionLog>();
        }

        public int AgencyId { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<ActionLog> ActionLogs { get; set; }
    }
}
