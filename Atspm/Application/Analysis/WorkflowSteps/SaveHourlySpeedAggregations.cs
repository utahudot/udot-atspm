using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// Persists <see cref="HourlySpeed"/> entries
    /// </summary>
    public class SaveHourlySpeedAggregations : TransformProcessStepBase<IEnumerable<HourlySpeed>, IEnumerable<HourlySpeed>>
    {
        private readonly IServiceScopeFactory _services;

        /// <summary>
        /// Persists <see cref="HourlySpeed"/> entries
        /// </summary>
        /// <param name="services"></param>
        /// <param name="dataflowBlockOptions"></param>
        public SaveHourlySpeedAggregations(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _services = services;
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<HourlySpeed>> Process(IEnumerable<HourlySpeed> input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            using (var scope = _services.CreateScope())
            {
                var repo = scope.ServiceProvider.GetService<IHourlySpeedRepository>();
                await repo.AddHourlySpeedsAsync(input.ToList());
                return input;
            }
        }
    }
}

