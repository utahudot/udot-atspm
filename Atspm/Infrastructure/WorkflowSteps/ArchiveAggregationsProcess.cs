#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.WorkflowSteps/ArchiveAggregationsProcess.cs
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
using System.Threading.Tasks.Dataflow;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.WorkflowSteps
{
    public class ArchiveAggregationsProcess(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformManyProcessStepBaseAsync<IEnumerable<AggregationModelBase>, CompressedAggregationBase>(dataflowBlockOptions)
    {
        protected override async IAsyncEnumerable<CompressedAggregationBase> Process(IEnumerable<AggregationModelBase> input, CancellationToken cancelToken = default)
        {
            var grouped = input.GroupBy(g => new GroupKey(
                g.LocationIdentifier,
                g.Start.Year,
                g.Start.Month,
                g.Start.Day,
                g.GetType()
            ));

            foreach (var group in grouped)
            {
                dynamic list = CreateTypedList(group, group.Key.DataType);
                var tl = new Timeline<StartEndRange>(group.Min(m => m.Start), group.Max(m => m.Start), TimeSpan.FromDays(1));

                var compressed = CreateCompressedAggregation(group.Key.DataType);
                compressed.LocationIdentifier = group.Key.LocationIdentifier;
                compressed.Start = tl.Start;
                compressed.End = tl.End;
                compressed.DataType = group.Key.DataType;
                compressed.Data = list;

                yield return compressed;
            }
        }

        private static IList CreateTypedList(IEnumerable<AggregationModelBase> source, Type type)
        {
            var listType = typeof(List<>).MakeGenericType(type);
            var list = (IList)Activator.CreateInstance(listType)!;

            foreach (var item in source)
                list.Add(item);

            return list;
        }

        private static CompressedAggregationBase CreateCompressedAggregation(Type type)
        {
            var compType = typeof(CompressedAggregations<>).MakeGenericType(type);
            return (CompressedAggregationBase)Activator.CreateInstance(compType)!;
        }

        private record GroupKey(string LocationIdentifier, int Year, int Month, int Day, Type DataType);
    }
}
