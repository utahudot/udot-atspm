#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.PreemptionDetails/PreemptDetailResult.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
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
        //public string LocationLocation { get; set; }

        //public ICollection<Plan> Plans { get; set; }

        //public ICollection<InputOn> InputOns { get; set; }
        //public ICollection<InputOff> InputOffs { get; set; }

        //public override string ToString()
        //{
        //    return JsonSerializer.Serialize(this);
        //}
    }
}
