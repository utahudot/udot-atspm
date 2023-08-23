using ATSPM.Data.Interfaces;
using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Data.Models
{
    public partial class ControllerLogArchive : ISignalLayer
    {
        public string SignalIdentifier { get; set; }
        public DateTime ArchiveDate { get; set; }

        public ICollection<ControllerEventLog> LogData { get; set; } = new List<ControllerEventLog>();

        public override string ToString()
        {
            return $"{SignalIdentifier}-{ArchiveDate:dd/MM/yyyy}-{LogData.Count}";
        }
    }
}
