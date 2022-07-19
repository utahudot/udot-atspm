using ATSPM.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Models
{
    public class DatabaseArchiveSettings : ApplicationSettings
    {
        public bool EnableDatbaseArchive { get; set; }
        public TableScheme? SelectedTableScheme { get; set; }
        public int? MonthsToKeepIndex { get; set; }
        public int? MonthsToKeepData { get; set; }
        public string ArchivePath { get; set; }
        public DeleteOrMove? SelectedDeleteOrMove { get; set; }
        public int? NumberOfRows { get; set; }
        public int? StartTime { get; set; }
        public int? TimeDuration { get; set; }
    }
}
