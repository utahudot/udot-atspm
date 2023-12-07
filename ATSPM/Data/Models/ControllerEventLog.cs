using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace ATSPM.Data.Models
{
    public class ControllerEventLog : ISignalLayer, ITimestamp
    {
        public string SignalIdentifier { get; set; }
        public DateTime Timestamp { get; set; }
        public int EventCode { get; set; }
        public int EventParam { get; set; }

        public override string ToString()
        {
            return $"{SignalIdentifier}-{EventCode}-{EventParam}-{Timestamp}";
        }
    }
}
