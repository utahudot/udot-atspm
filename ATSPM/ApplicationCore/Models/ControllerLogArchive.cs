using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Application.Models
{
    public partial class ControllerLogArchive : ATSPMModelBase
    {
        public string SignalId { get; set; }
        public DateTime ArchiveDate { get; set; }
        //public byte[] LogData { get; set; }

        public IList<ControllerEventLog> LogData { get; set; } = new List<ControllerEventLog>();

        public override string ToString()
        {
            return $"{SignalId} - {ArchiveDate.ToString("dd/MM/yyyy")} - {LogData.Count}";
        }
    }
}
