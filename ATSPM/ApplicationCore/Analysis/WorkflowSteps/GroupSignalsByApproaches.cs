using ATSPM.Application.Specifications;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    /// <summary>
    /// Breaks out all <see cref="Approach"/> from <see cref="Location"/>
    /// and returns separate Tuples of <see cref="Approach"/>/<see cref="ControllerEventLog"/> pairs
    /// sorted by <see cref="ControllerEventLog.Timestamp"/>.
    /// </summary>
    public class GroupLocationsByApproaches : TransformManyProcessStepBase<Tuple<Location, IEnumerable<ControllerEventLog>>, Tuple<Approach, IEnumerable<ControllerEventLog>>>
    {
        /// <inheritdoc/>
        public GroupLocationsByApproaches(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<Tuple<Approach, IEnumerable<ControllerEventLog>>>> Process(Tuple<Location, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var Location = input.Item1;
            var logs = input.Item2;
            
            var result = Location.Approaches.Select(s => Tuple.Create(s, logs.FromSpecification(new ControllerLogLocationFilterSpecification(Location))));

            return Task.FromResult(result);
        }
    }
}
