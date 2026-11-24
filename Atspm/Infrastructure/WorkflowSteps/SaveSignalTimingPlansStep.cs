#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.WorkflowSteps/SaveSignalTimingPlansStep.cs
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
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// A workflow step responsible for atomically merging, reconciling, and persisting signal timing plans per location in the repository.
    /// </summary>
    public class SaveSignalTimingPlansStep(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformManyProcessStepBaseAsync<IEnumerable<SignalTimingPlan>, SignalTimingPlan>(dataflowBlockOptions)
    {
        private static readonly long TimestampEpochTicks = new DateTime(2000, 1, 1).Ticks;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locationLocks = new();
        private readonly IServiceScopeFactory _services = services;

        /// <inheritdoc/>
        protected override async IAsyncEnumerable<SignalTimingPlan> Process(IEnumerable<SignalTimingPlan> input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var groups = input.GroupBy(p => p.LocationIdentifier).ToList();
            if (groups.Count == 0) yield break;

            foreach (var group in groups)
            {
                var locationId = group.Key ?? string.Empty;
                var locationLock = _locationLocks.GetOrAdd(locationId, _ => new SemaphoreSlim(1, 1));

                List<SignalTimingPlan> savedPlans;

                await locationLock.WaitAsync(cancelToken);
                try
                {
                    using var scope = _services.CreateAsyncScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ISignalTimingPlanRepository>();
                    var options = scope.ServiceProvider.GetService<IOptions<DeviceEventLoggingConfiguration>>();
                    var planOffset = options?.Value.SignalTimingPlanOffsetHours ?? 12;
                    var context = scope.ServiceProvider.GetService<AggregationContext>();
                    var planType = context?.Model.FindEntityType(typeof(SignalTimingPlan));
                    var startStorage = GetTimestampStorage(planType?.FindProperty(nameof(SignalTimingPlan.Start)));
                    var endStorage = GetTimestampStorage(planType?.FindProperty(nameof(SignalTimingPlan.End)));
                    var plans = startStorage.TickResolution == 1 && endStorage.TickResolution == 1
                        ? group.ToList()
                        : group.Select(plan => new SignalTimingPlan
                        {
                            LocationIdentifier = plan.LocationIdentifier,
                            PlanNumber = plan.PlanNumber,
                            Start = NormalizeTimestamp(plan.Start, startStorage),
                            End = NormalizeTimestamp(plan.End, endStorage)
                        }).ToList();

                    var minStart = plans.Min(p => p.Start).AddHours(-planOffset);
                    var maxStart = plans.Max(p => p.Start).AddHours(planOffset);

                    var existingAll = new List<SignalTimingPlan>();
                    var queryable = repo.GetList();

                    if (queryable != null)
                    {
                        var existingInWindow = await queryable
                            .AsNoTracking()
                            .Where(w => w.LocationIdentifier == group.Key
                                && w.Start >= minStart
                                && w.Start < maxStart)
                            .ToListAsync(cancelToken);

                        var previousNeighbor = await queryable
                            .AsNoTracking()
                            .Where(w => w.LocationIdentifier == group.Key && w.Start < minStart)
                            .OrderByDescending(w => w.Start)
                            .FirstOrDefaultAsync(cancelToken);

                        var nextNeighbor = await queryable
                            .AsNoTracking()
                            .Where(w => w.LocationIdentifier == group.Key && w.Start >= maxStart)
                            .OrderBy(w => w.Start)
                            .FirstOrDefaultAsync(cancelToken);

                        existingAll = existingInWindow
                            .Concat(previousNeighbor != null ? [previousNeighbor] : [])
                            .Concat(nextNeighbor != null ? [nextNeighbor] : [])
                            .DistinctBy(p => (p.LocationIdentifier, p.PlanNumber, p.Start))
                            .ToList();
                    }

                    var combined = existingAll
                        .Concat(plans)
                        .GroupBy(p => (p.LocationIdentifier, p.PlanNumber, p.Start))
                        .Select(g => new SignalTimingPlan
                        {
                            LocationIdentifier = g.Key.LocationIdentifier,
                            PlanNumber = g.Key.PlanNumber,
                            Start = g.Key.Start,
                            End = g.Max(x => x.End)
                        })
                        .OrderBy(p => p.Start)
                        .ToList();

                    savedPlans = combined.Zip(combined.Skip(1).Append(null), (current, next) =>
                    {
                        if (next != null)
                        {
                            current.End = next.Start;
                        }
                        return current;
                    }).ToList();

                    foreach (var plan in savedPlans)
                    {
                        var existing = await repo.LookupAsync(plan);

                        if (existing != null)
                        {
                            if (existing.End != plan.End)
                            {
                                existing.End = plan.End;
                                await repo.UpdateAsync(existing);
                            }
                        }
                        else
                        {
                            await repo.AddAsync(plan);
                        }
                    }
                }
                finally
                {
                    locationLock.Release();
                }

                foreach (var plan in savedPlans)
                {
                    yield return plan;
                }
            }
        }

        private static (long TickResolution, long EpochTicks) GetTimestampStorage(IProperty property)
        {
            var mapping = property?.GetTypeMapping() as RelationalTypeMapping;
            // The unqualified timestamp mapping uses signed microseconds relative to 2000-01-01.
            var usesTimestampEpoch = string.Equals(mapping?.StoreType, "timestamp", StringComparison.OrdinalIgnoreCase);
            var fractionalDigits = property?.GetPrecision() ?? mapping?.Precision ?? (usesTimestampEpoch ? 6 : 7);
            var tickResolution = (long)Math.Pow(10, Math.Max(0, 7 - fractionalDigits));
            return (tickResolution, usesTimestampEpoch ? TimestampEpochTicks : 0);
        }

        private static DateTime NormalizeTimestamp(DateTime timestamp, (long TickResolution, long EpochTicks) storage)
        {
            if (timestamp == DateTime.MinValue || timestamp == DateTime.MaxValue)
                return timestamp;

            var extraTicks = (timestamp.Ticks - storage.EpochTicks) % storage.TickResolution;
            return timestamp.AddTicks(-extraTicks);
        }
    }
}
