using System;
using System.Collections.Generic;

#nullable disable

namespace ControllerLogger.Models
{
    public partial class ControllerLogArchive
    {
        public string SignalId { get; set; }
        public DateTime ArchiveDate { get; set; }
        public byte[] LogData { get; set; }
    }
}
