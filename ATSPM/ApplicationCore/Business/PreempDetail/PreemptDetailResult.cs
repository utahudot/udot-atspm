#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PreempDetail/PreemptDetailResult.cs
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
using ATSPM.Application.Business.Common;
using ATSPM.Application.TempModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.PreempDetail
{
    /// <summary>
    /// Preempt Detail chart
    /// </summary>
    public class PreemptDetailResult
    {
        public PreemptDetailResult(
            ICollection<PreemptDetail> preemptDetails,
            PreemptRequestAndServices preemptSummary)
        {
            Details = preemptDetails;
            Summary = preemptSummary;
        }
        public ICollection<PreemptDetail> Details { get; set; }

        public PreemptRequestAndServices Summary { get; set; }
    }

    public class PreemptDetail : LocationResult
    {
        public PreemptDetail(
            string locationId,
            DateTime start,
            DateTime end,
            int preemptNumber,
            ICollection<PreemptCycleResult> preemptCycles) : base(locationId, start, end)
        {
            PreemptionNumber = preemptNumber;
            Cycles = preemptCycles;
        }
        public int PreemptionNumber { get; set; }
        public ICollection<PreemptCycleResult> Cycles { get; }
    }
}