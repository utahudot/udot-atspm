using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace ControllerLogger.Models
{
    public partial class ControllerEventLog
    {
        //[Key]
        //[Column("SignalID")]
        public string SignalId { get; set; }
        //[Key]
        //[Column("ArchiveDate")]
        public DateTime Timestamp { get; set; }
        public int EventCode { get; set; }
        public int EventParam { get; set; }

        public override string ToString()
        {
            return $"{SignalId}-{EventCode}-{EventParam}-{Timestamp}";
        }
    }
}
