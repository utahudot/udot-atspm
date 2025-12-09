#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/IndianaPedDataSpecification.cs
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
    /// <item><see cref="IndianaEnumerations.PhaseOn"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianBeginWalk"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianBeginChangeInterval"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianOverlapBeginWalk"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianOverlapBeginClearance"/></item>
    /// <item><see cref="IndianaEnumerations.PedDetectorOn"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianCallRegistered"/></item>
    /// </list>
    /// </summary>
    public class IndianaPedDataSpecification : BaseSpecification<IndianaEvent>
    {
        /// <inheritdoc cref="IndianaPedDataSpecification"/>
        public IndianaPedDataSpecification()
        {
            var codes = new HashSet<short>()
            {
                (short)IndianaEnumerations.PhaseOn,
                (short)IndianaEnumerations.PedestrianBeginWalk,
                (short)IndianaEnumerations.PedestrianBeginChangeInterval,
                (short)IndianaEnumerations.PedestrianOverlapBeginWalk,
                (short)IndianaEnumerations.PedestrianOverlapBeginClearance,
                (short)IndianaEnumerations.PedDetectorOn,
                (short)IndianaEnumerations.PedestrianCallRegistered
            };

            Criteria = c => codes.Contains(c.EventCode);

            ApplyOrderBy(o => o.Timestamp);
        }
    }
}
