using ATSPM.Data.Interfaces;
using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Data.Models
{
    public partial class ControllerLogArchive : ILocationLayer
    {
        public string LocationIdentifier { get; set; }
        public DateTime ArchiveDate { get; set; }

        public ICollection<ControllerEventLog> LogData { get; set; } = new List<ControllerEventLog>();

        public override string ToString()
        {
            return $"{LocationIdentifier}-{ArchiveDate:dd/MM/yyyy}-{LogData.Count}";
        }
    }
}
