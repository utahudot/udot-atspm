#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.WorkflowSteps/MergeExistingSignalPlansStep.cs
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
            if (!groups.Any()) yield return Enumerable.Empty<SignalTimingPlan>();

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
