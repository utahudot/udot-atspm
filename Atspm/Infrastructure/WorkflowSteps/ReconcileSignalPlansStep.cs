using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// A workflow step that reconciles timing plans by setting the end time of a plan to the start time of the subsequent plan.
    /// </summary>
    public class ReconcileSignalPlansStep(ExecutionDataflowBlockOptions options) : TransformManyProcessStepBaseAsync<IEnumerable<SignalTimingPlan>, IEnumerable<SignalTimingPlan>>(options)
    {
        protected override async IAsyncEnumerable<IEnumerable<SignalTimingPlan>> Process(IEnumerable<SignalTimingPlan> input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var groups = input.GroupBy(g => (g.LocationIdentifier, g.PlanNumber));

            foreach (var g in groups)
            {
                var ordered = g.OrderBy(p => p.Start).ToList();

                var finalized = ordered.Zip(ordered.Skip(1).Append(null), (current, next) =>
                {
                    current.End = next?.Start ?? DateTime.MinValue;
                    return current;
                });

                yield return finalized;
            }
        }
    }
}
