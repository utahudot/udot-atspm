using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class ApplicationEvent
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ApplicationName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int SeverityLevel { get; set; }
        public string? Class { get; set; }
        public string? Function { get; set; }
    }
}
