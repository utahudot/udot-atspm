#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/GroupApproachesByLocation.cs
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
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// Breaks out all <see cref="Approach"/> from <see cref="Location"/>
    /// and returns separate Tuples of <see cref="Approach"/>/<see cref="IEnumerable{IndianaEvent}"/> pairs
    /// sorted by <see cref="ITimestamp.Timestamp"/>.
    /// </summary>
    public class GroupApproachesByLocation : TransformManyProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, Tuple<Approach, int, IEnumerable<IndianaEvent>>>
    {
        /// <inheritdoc/>
        public GroupApproachesByLocation(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<Tuple<Approach, int, IEnumerable<IndianaEvent>>>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var location = input.Item1;
            var events = input.Item2
                .FromSpecification(new EventLogSpecification(location))
                .Cast<IndianaEvent>()
                .ToList()
                .AsEnumerable();

            //foreach (var e in events)
            //{
            //    var en = (IndianaEnumerations)e.EventCode;
            //    var att = en.GetAttributeOfType<IndianaEventLayerAttribute>();

            //    if (att.IndianaEventParamType == IndianaEventParamType.PhaseNumber && )
            //    {

            //    }
            //}

            var result = location.Approaches.Select(s => Tuple.Create(s, s.ProtectedPhaseNumber, events));

            return Task.FromResult(result);
        }
    }
}
