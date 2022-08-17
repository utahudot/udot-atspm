using ATSPM.Application.Enums;
using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Application.Models
{
    public partial class ApplicationEvent : ATSPMModelBase
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ApplicationName { get; set; }
        public string Description { get; set; }
        public SeverityLevels SeverityLevel { get; set; }
        public string Class { get; set; }
        public string Function { get; set; }
    }
}
