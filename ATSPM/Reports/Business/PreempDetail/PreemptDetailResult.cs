using ATSPM.Application.Reports.Business.Common;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PreempDetail
{
    /// <summary>
    /// Preempt Detail chart
    /// </summary>
    public class PreemptDetailResult:SignalResult
    {
        public PreemptDetailResult(
            string signalId,
            DateTime start,
            DateTime end,
            int preemptNumber,
            ICollection<Plan> plans,
            ICollection<Delay> delay,
            ICollection<ServiceTime> serviceTimes,
            ICollection<TrackClearTime> trackClearTimes,
            ICollection<DwellTime> dwellTimes,
            ICollection<CallMaxOutTime> callMaxOutTimes,
            ICollection<GateDownTime> gateDownTimes,
            ICollection<InputOn> inputOns,
            ICollection<InputOff> inputOffs):base(signalId, start, end)
        {
            PreemptNumber = preemptNumber;
            Plans = plans;
            Delay = delay;
            ServiceTimes = serviceTimes;
            TrackClearTimes = trackClearTimes;
            DwellTimes = dwellTimes;
            CallMaxOutTimes = callMaxOutTimes;
            GateDownTimes = gateDownTimes;
            InputOns = inputOns;
            InputOffs = inputOffs;
        }
        public int PreemptNumber { get; set; }
        public ICollection<Plan> Plans { get; set; }
        public ICollection<Delay> Delay { get; set; }
        public ICollection<ServiceTime> ServiceTimes { get; set; }
        public ICollection<TrackClearTime> TrackClearTimes { get; set; }
        public ICollection<DwellTime> DwellTimes { get; set; }
        public ICollection<CallMaxOutTime> CallMaxOutTimes { get; set; }
        public ICollection<GateDownTime> GateDownTimes { get; set; }
        public ICollection<InputOn> InputOns { get; set; }
        public ICollection<InputOff> InputOffs { get; set; }
    }
}