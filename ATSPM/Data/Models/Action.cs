using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class Action
    {
        public Action()
        {
            ActionLogActionLogs = new HashSet<ActionLog>();
        }

        public int ActionId { get; set; }
        public string Description { get; set; } = null!;

        public virtual ICollection<ActionLog> ActionLogActionLogs { get; set; }
    }
}
