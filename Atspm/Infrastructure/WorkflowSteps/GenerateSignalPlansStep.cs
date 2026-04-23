using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// A workflow step that groups Indiana events by location and parameter to generate initial signal timing plans.
    /// </summary>
    public class GenerateSignalPlansStep(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformManyProcessStepBaseAsync<IEnumerable<IndianaEvent>, IEnumerable<SignalTimingPlan>>(dataflowBlockOptions)
    {
        protected override async IAsyncEnumerable<IEnumerable<SignalTimingPlan>> Process(IEnumerable<IndianaEvent> input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var groups = input
                .FromSpecification(new IndianaPlanDataSpecification())
                .GroupBy(e => (e.LocationIdentifier, e.EventParam));

            foreach (var g in groups)
            {
                var unique = g.KeepFirstSequentialParam().ToList();

                if (unique.Count > 0)
                {
                    var chunk = unique.Select(s => new SignalTimingPlan
                    {
                        LocationIdentifier = s.LocationIdentifier,
                        PlanNumber = s.EventParam,
                        Start = s.Timestamp,
                        End = DateTime.MinValue
                    }).ToList();

                    yield return chunk;
                }
            }
        }
    }
}
