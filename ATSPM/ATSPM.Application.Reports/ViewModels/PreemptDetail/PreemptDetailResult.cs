using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.PreemptDetail
{
    /// <summary>
    /// Preempt Detail chart
    /// </summary>
    public class PreemptDetailResult
    {
        public PreemptDetailResult(
            string chartName,
            string signalId,
            string signalLocation,
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
            ICollection<InputOff> inputOffs)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            Start = start;
            End = end;
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

        public string ChartName { get; set; }
        public string SignalId { get; set; }
        public string SignalLocation { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
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