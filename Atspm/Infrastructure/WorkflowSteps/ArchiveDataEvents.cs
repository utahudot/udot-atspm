#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.WorkflowSteps/ArchiveDataEvents.cs
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

using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// returns a <see cref="CompressedEventLogs{T}"/> object to store to a repository that is grouped by:
    /// <list type="bullet">
    /// <item><see cref="ILocationLayer.LocationIdentifier"/></item>
    /// <item><see cref="ITimestamp.Timestamp"/></item>
    /// <item><see cref="Device"/></item>
    /// <item>Event type derived from <see cref="EventLogModelBase"/></item>
    /// </list>
    /// </summary>
    public class ArchiveDataEvents : TransformManyProcessStepBaseAsync<Tuple<Device, EventLogModelBase>[], CompressedEventLogBase>
    {
        /// <summary>
        /// returns a <see cref="CompressedEventLogs{T}"/> object to store to a repository that is grouped by:
        /// <list type="bullet">
        /// <item><see cref="ILocationLayer.LocationIdentifier"/></item>
        /// <item><see cref="ITimestamp.Timestamp"/></item>
        /// <item><see cref="Device"/></item>
        /// <item>Event type derived from <see cref="EventLogModelBase"/></item>
        /// </list>
        /// </summary>
        /// <param name="dataflowBlockOptions"></param>
        public ArchiveDataEvents(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override async IAsyncEnumerable<CompressedEventLogBase> Process(Tuple<Device, EventLogModelBase>[] input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => (g.Item2.LocationIdentifier, g.Item2.Timestamp.Date, g.Item1.Id, g.Item2.GetType()))
                .Select(s =>
                {
                    dynamic list = Activator.CreateInstance(typeof(List<>).MakeGenericType(s.Key.Item4));

                    foreach (var i in s.Select(s => s.Item2))
                    {
                        if (list is IList l)
                        {
                            l.Add(i);
                        }
                    }

                    dynamic comp = Activator.CreateInstance(typeof(CompressedEventLogs<>).MakeGenericType(s.Key.Item4));

                    comp.LocationIdentifier = s.Key.LocationIdentifier;
                    comp.ArchiveDate = DateOnly.FromDateTime(s.Key.Date);
                    comp.DataType = s.Key.Item4;
                    comp.DeviceId = s.Key.Id;
                    comp.Data = list;

                    return comp;
                });

            foreach (var r in result)
            {
                yield return r;
            }
        }
    }
}
