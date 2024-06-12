#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PreemptService/PreemptServiceResult.cs
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

namespace ATSPM.Application.Business.PreemptService
{
    /// <summary>
    /// Preempt Service chart
    /// </summary>
    public class PreemptServiceResult : LocationResult
    {
        public PreemptServiceResult(
            string locationId,
            DateTime start,
            DateTime end,
            ICollection<PreemptPlan> plans,
            ICollection<DataPointForInt> preemptServiceEvents) : base(locationId, start, end)
        {
            Plans = plans;
            PreemptServiceEvents = preemptServiceEvents;
        }
        public ICollection<PreemptPlan> Plans { get; }
        public ICollection<DataPointForInt> PreemptServiceEvents { get; }
    }
}