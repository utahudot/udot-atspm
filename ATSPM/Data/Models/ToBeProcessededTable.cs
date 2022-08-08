using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class ToBeProcessededTable
    {
        public int Id { get; set; }
        public string PartitionedTableName { get; set; } = null!;
        public DateTime UpdatedTime { get; set; }
        public string PreserveDataSelect { get; set; } = null!;
        public int TableId { get; set; }
        public string PreserveDataWhere { get; set; } = null!;
        public string InsertValues { get; set; } = null!;
        public string DataBaseName { get; set; } = null!;
        public bool Verbose { get; set; }
        public string CreateColumns4Table { get; set; } = null!;
    }
}
