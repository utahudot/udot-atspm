using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class ToBeProcessedTableIndex
    {
        public int Id { get; set; }
        public int? TableId { get; set; }
        public int? IndexId { get; set; }
        public string? ClusteredText { get; set; }
        public string? TextForIndex { get; set; }
        public string? IndexName { get; set; }
    }
}
