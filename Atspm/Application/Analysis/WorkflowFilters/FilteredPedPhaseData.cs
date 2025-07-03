#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowFilters/FilteredPedPhaseData.cs
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

using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="IndianaEvent"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PedestrianBeginWalk"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianBeginChangeInterval"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianOverlapBeginWalk"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianOverlapBeginClearance"/></item>
    /// </list>
    /// </summary>
    public class FilteredPedPhaseData : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilteredPedPhaseData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.PedestrianBeginWalk);
            filteredList.Add((int)IndianaEnumerations.PedestrianBeginChangeInterval);
            filteredList.Add((int)IndianaEnumerations.PedestrianOverlapBeginWalk);
            filteredList.Add((int)IndianaEnumerations.PedestrianOverlapBeginClearance);
        }
    }
}
