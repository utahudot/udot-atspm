using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// A workflow step that retrieves existing plans from the repository to merge with the current processing batch.
    /// </summary>
    public class MergeExistingSignalPlansStep(IServiceScopeFactory services, ExecutionDataflowBlockOptions options) : TransformManyProcessStepBaseAsync<IEnumerable<SignalTimingPlan>, IEnumerable<SignalTimingPlan>>(options)
    {
        private readonly IServiceScopeFactory _services = services;

        /// <inheritdoc/>
        protected override async IAsyncEnumerable<IEnumerable<SignalTimingPlan>> Process(IEnumerable<SignalTimingPlan> input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var groups = input.GroupBy(g => (g.LocationIdentifier, g.PlanNumber));
            if (!groups.Any()) yield break;

            using var scope = _services.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetService<ISignalTimingPlanRepository>();

            var options = scope.ServiceProvider.GetService<IOptions<DeviceEventLoggingConfiguration>>();
            var planOffset = options?.Value.SignalTimingPlanOffsetHours ?? 12;

            foreach (var g in groups)
            {
                var minStart = g.Min(p => p.Start).AddHours(-planOffset);
                var maxStart = g.Max(p => p.Start).AddHours(planOffset);

                var existing = await repo.GetList()
                    .AsNoTracking()
                    .Where(w => w.LocationIdentifier == g.Key.LocationIdentifier
                    && w.PlanNumber == g.Key.PlanNumber
                    && w.Start >= minStart
                    && w.Start < maxStart)
                    .ToListAsync(cancelToken);

                var combined = g.Concat(existing).DistinctBy(p => new { p.LocationIdentifier, p.PlanNumber, p.Start }).ToList();

                yield return combined;
            }
        }
    }
}
