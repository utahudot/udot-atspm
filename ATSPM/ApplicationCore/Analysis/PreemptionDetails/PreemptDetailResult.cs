using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ATSPM.Application.Analysis.PreemptionDetails
{
    public class PreemptDetailResult : PreempDetailValueBase
    {
        public IReadOnlyList<DwellTimeValue> DwellTimes { get; set; }
        public IReadOnlyList<TrackClearTimeValue> TrackClearTimes { get; set; }
        public IReadOnlyList<TimeToServiceValue> ServiceTimes { get; set; }
        public IReadOnlyList<DelayTimeValue> Delay { get; set; }
        public IReadOnlyList<TimeToGateDownValue> GateDownTimes { get; set; }
        public IReadOnlyList<TimeToCallMaxOutValue> CallMaxOutTimes { get; set; }



        //public string ChartName { get; set; }
        //public string SignalLocation { get; set; }

        //public ICollection<Plan> Plans { get; set; }

        //public ICollection<InputOn> InputOns { get; set; }
        //public ICollection<InputOff> InputOffs { get; set; }

        //public override string ToString()
        //{
        //    return JsonSerializer.Serialize(this);
        //}
    }
}
