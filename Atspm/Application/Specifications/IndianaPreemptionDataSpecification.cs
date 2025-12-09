#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/IndianaPreemptionDataSpecification.cs
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

using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Specifications;

namespace Utah.Udot.Atspm.Specifications
{
    /// <summary>
    /// Filters <see cref="IEnumerable{IndianaEvent}"/> by:
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PreemptCallInputOn"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptGateDownInputReceived"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptCallInputOff"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptEntryStarted"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptionBeginTrackClearance"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptionBeginDwellService"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptionMaxPresenceExceeded"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptionBeginExitInterval"/></item>
    /// </list>
    /// </summary>
    public class IndianaPreemptionDataSpecification : BaseSpecification<IndianaEvent>
    {
        /// <inheritdoc cref="IndianaPreemptionDataSpecification"/>
        public IndianaPreemptionDataSpecification()
        {
            var codes = new HashSet<short>()
            {
                (short)IndianaEnumerations.PreemptCallInputOn,
                (short)IndianaEnumerations.PreemptGateDownInputReceived,
                (short)IndianaEnumerations.PreemptCallInputOff,
                (short)IndianaEnumerations.PreemptEntryStarted,
                (short)IndianaEnumerations.PreemptionBeginTrackClearance,
                (short)IndianaEnumerations.PreemptionBeginDwellService,
                (short)IndianaEnumerations.PreemptionMaxPresenceExceeded,
                (short)IndianaEnumerations.PreemptionBeginExitInterval,
            };

            Criteria = c => codes.Contains(c.EventCode);

            ApplyOrderBy(o => o.Timestamp);
        }
    }
}
