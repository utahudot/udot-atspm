using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class ToBeProcessededIndex
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public int IndexId { get; set; }
        public string ClusterText { get; set; } = null!;
        public string TextForIndex { get; set; } = null!;
        public string IndexName { get; set; } = null!;
    }
}
