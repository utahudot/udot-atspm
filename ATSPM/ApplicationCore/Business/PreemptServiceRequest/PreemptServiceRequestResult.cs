#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PreemptServiceRequest/PreemptServiceRequestResult.cs
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
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.PreemptServiceRequest
{
    /// <summary>
    /// Preempt Service Request chart
    /// </summary>
    public class PreemptServiceRequestResult : LocationResult
    {
        public PreemptServiceRequestResult(string chartName,
            string locationId,
            DateTime start,
            DateTime end,
            IReadOnlyList<Plan> plans,
            IReadOnlyList<DataPointForInt> preemptRequests) : base(locationId, start, end)
        {
            Plans = plans;
            PreemptRequests = preemptRequests;
        }
        public IReadOnlyList<Plan> Plans { get; internal set; }
        public IReadOnlyList<DataPointForInt> PreemptRequests { get; internal set; }
    }
}