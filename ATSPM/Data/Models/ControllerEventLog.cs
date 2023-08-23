using ATSPM.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace ATSPM.Data.Models
{
    public class ControllerEventLog : ISignalLayer
    {
        public string SignalIdentifier { get; set; }
        public DateTime TimeStamp { get; set; }
        public int EventCode { get; set; }
        public int EventParam { get; set; }

        public override string ToString()
        {
            return $"{SignalIdentifier}-{EventCode}-{EventParam}-{TimeStamp}";
        }
    }
}
