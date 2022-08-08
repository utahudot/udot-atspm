using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace ATSPM.Application.ValueObjects
{
    public class ControllerEventLog
    {
        public string SignalId { get; set; }
        public DateTime Timestamp { get; set; }
        public int EventCode { get; set; }
        public int EventParam { get; set; }

        public override string ToString()
        {
            return $"{SignalId}-{EventCode}-{EventParam}-{Timestamp}";
        }
    }
}
