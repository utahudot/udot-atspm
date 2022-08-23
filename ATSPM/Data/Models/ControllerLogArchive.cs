using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Data.Models
{
    public partial class ControllerLogArchive : ATSPMModelBase
    {
        public string SignalId { get; set; }
        public DateTime ArchiveDate { get; set; }

        public ICollection<ControllerEventLog> LogData { get; set; } = new List<ControllerEventLog>();

        public override string ToString()
        {
            return $"{SignalId}-{ArchiveDate:dd/MM/yyyy}-{LogData.Count}";
        }
    }
}
