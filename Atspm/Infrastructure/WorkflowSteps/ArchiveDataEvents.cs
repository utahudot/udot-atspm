#region license
// Copyright 2025 Utah Departement of Transportation
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
            var group = input.GroupBy(g =>
            (g.Item2.LocationIdentifier,
            g.Item2.Timestamp.Year,
            g.Item2.Timestamp.Month,
            g.Item2.Timestamp.Day,
            g.Item2.Timestamp.Hour,
            g.Item1.Id,
            g.Item2.GetType()));

            foreach (var g in group)
            {
                dynamic list = Activator.CreateInstance(typeof(List<>).MakeGenericType(g.Key.Item7));

                foreach (var i in g)
                {
                    if (list is IList l)
                    {
                        l.Add(i.Item2);
                    }
                }

                var tl = new Timeline<StartEndRange>(list, TimeSpan.FromHours(1));

                dynamic comp = Activator.CreateInstance(typeof(CompressedEventLogs<>).MakeGenericType(g.Key.Item7));

                comp.LocationIdentifier = g.Key.LocationIdentifier;
                //comp.ArchiveDate = DateOnly.FromDateTime(s.Key.Date);
                comp.Start = tl.Start;
                comp.End = tl.End;
                comp.DataType = g.Key.Item7;
                comp.DeviceId = g.Key.Id;
                comp.Data = list;


                yield return comp;
            }
        }
    }

    //TODO: change to this from AI suggestion
    //protected override async IAsyncEnumerable<CompressedEventLogBase> Process(
    //Tuple<Device, EventLogModelBase>[] input,
    //[EnumeratorCancellation] CancellationToken cancelToken = default)
    //    {
    //        var grouped = input.GroupBy(g => new
    //        {
    //            g.Item2.LocationIdentifier,
    //            g.Item2.Timestamp.Year,
    //            g.Item2.Timestamp.Month,
    //            g.Item2.Timestamp.Day,
    //            g.Item2.Timestamp.Hour,
    //            DeviceId = g.Item1.Id,
    //            DataType = g.Item2.GetType()
    //        });

    //        foreach (var group in grouped)
    //        {
    //            var list = CreateTypedList(group.Select(x => x.Item2), group.Key.DataType);

    //            var timeline = new Timeline<StartEndRange>((IList)list, TimeSpan.FromHours(1));

    //            var compressed = CreateCompressedEventLog(group.Key.DataType);
    //            compressed.LocationIdentifier = group.Key.LocationIdentifier;
    //            compressed.Start = timeline.Start;
    //            compressed.End = timeline.End;
    //            compressed.DataType = group.Key.DataType;
    //            compressed.DeviceId = group.Key.DeviceId;
    //            compressed.Data = list;

    //            yield return compressed;
    //        }
    //    }

    //private static object CreateTypedList(IEnumerable<EventLogModelBase> source, Type type)
    //    {
    //        var listType = typeof(List<>).MakeGenericType(type);
    //        var list = (IList)Activator.CreateInstance(listType)!;

    //        foreach (var item in source)
    //            list.Add(item);

    //        return list;
    //    }

    //    private static CompressedEventLogBase CreateCompressedEventLog(Type type)
    //    {
    //        var compType = typeof(CompressedEventLogs<>).MakeGenericType(type);
    //        return (CompressedEventLogBase)Activator.CreateInstance(compType)!;
    //    }

}
