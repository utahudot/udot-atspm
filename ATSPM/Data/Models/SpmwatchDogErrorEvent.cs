using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class SpmwatchDogErrorEvent
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string SignalId { get; set; } = null!;
        public string? DetectorId { get; set; }
        public string Direction { get; set; } = null!;
        public int Phase { get; set; }
        public int ErrorCode { get; set; }
        public string Message { get; set; } = null!;
    }
}
