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
                var tl = new Timeline<StartEndRange>(group, TimeSpan.FromDays(1));

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

        //TODO: update this to Utah.Udot.ATSPM.Infrastructure.WorkflowSteps.ArchiveDataEvents
        private record GroupKey(string LocationIdentifier, int Year, int Month, int Day, Type DataType);
    }
}
