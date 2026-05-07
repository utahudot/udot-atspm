using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// A workflow step responsible for persisting new signal timing plans or updating the end times of existing plans in the repository.
    /// </summary>
    public class SaveSignalTimingPlansStep(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformManyProcessStepBaseAsync<IEnumerable<SignalTimingPlan>, SignalTimingPlan>(dataflowBlockOptions)
    {
        private readonly IServiceScopeFactory _services = services;

        protected override async IAsyncEnumerable<SignalTimingPlan> Process(IEnumerable<SignalTimingPlan> input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            if (!input.Any()) yield break;

            using var scope = _services.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<ISignalTimingPlanRepository>();

            foreach (var i in input)
            {
                var existing = await repo.LookupAsync(i);

                if (existing != null)
                {
                    if (existing.End != i.End)
                    {
                        existing.End = i.End;
                        await repo.UpdateAsync(existing);
                    }
                }
                else
                {
                    await repo.AddAsync(i);
                }

                yield return i;
            }
        }
    }
}
