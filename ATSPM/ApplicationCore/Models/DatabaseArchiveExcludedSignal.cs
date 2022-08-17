using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Application.Models
{
    public partial class DatabaseArchiveExcludedSignal : ATSPMModelBase
    {
        public int Id { get; set; }
        public string SignalId { get; set; }
        public string SignalDescription { get; set; }
    }
}
