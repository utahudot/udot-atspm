using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Data.Models
{
    public partial class ControllerLogArchive : ATSPMModelBase
    {
        public string SignalID { get; set; }
        public DateTime ArchiveDate { get; set; }

        public IList<ControllerEventLog> LogData { get; set; } = new List<ControllerEventLog>();

        public override string ToString()
        {
            return $"{SignalID}-{ArchiveDate:dd/MM/yyyy}-{LogData.Count}";
        }
    }
}
