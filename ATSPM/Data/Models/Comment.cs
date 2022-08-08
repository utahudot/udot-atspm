using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class Comment
    {
        public long CommentId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Entity { get; set; } = null!;
        public int ChartType { get; set; }
        public string Comment1 { get; set; } = null!;
        public int? EntityType { get; set; }
    }
}
