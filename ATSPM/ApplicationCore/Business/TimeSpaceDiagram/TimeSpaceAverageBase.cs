using ATSPM.Application.Business.Common;
using ATSPM.Data.Models.EventLogModels;
using System.Collections.Generic;

namespace ATSPM.Application.Business.TimeSpaceDiagram
{
    public class TimeSpaceAverageBase
    {
        public List<List<IndianaEvent>> ControllerEventLogsList { get; set; }
        public List<PhaseDetail> PrimaryPhaseDetails { get; set; }
        public List<PhaseDetail> OpposingPhaseDetails { get; set; }
        public List<IndianaEvent> ProgramSplits { get; set; }
        public int ProgrammedCycleLength { get; set; }
        public int Offset { get; set; }
    }
}
