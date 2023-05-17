using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Analysis.PreemptionDetails
{
    public class PreemptDetailResult
    {
        public string SignalId { get; set; }
        public int PreemptNumber { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public IEnumerable<DwellTimeValue> DwellTimes { get; set; }
        public IEnumerable<TrackClearTimeValue> TrackClearTimes { get; set; }
        public IEnumerable<TimeToServiceValue> ServiceTimes { get; set; }
        public IEnumerable<DelayTimeValue> Delay { get; set; }
        public IEnumerable<TimeToGateDownValue> GateDownTimes { get; set; }
        public IEnumerable<TimeToCallMaxOutValue> CallMaxOutTimes { get; set; }

        public override string ToString() => $"{GetType().Name}-{SignalId}-{PreemptNumber}-{Start}-{End}-{DwellTimes.Count()}-{TrackClearTimes?.Count()}-{ServiceTimes?.Count()}-{Delay?.Count()}-{GateDownTimes?.Count()}-{CallMaxOutTimes?.Count()}";

        //public string ChartName { get; set; }
        //public string SignalLocation { get; set; }

        //public ICollection<Plan> Plans { get; set; }

        //public ICollection<InputOn> InputOns { get; set; }
        //public ICollection<InputOff> InputOffs { get; set; }
    }
}
