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
    /// Breaks out all <see cref="Approach"/> from <see cref="Signal"/>
    /// and returns separate Tuples of <see cref="Approach"/>/<see cref="ControllerEventLog"/> pairs
    /// sorted by <see cref="ControllerEventLog.Timestamp"/>.
    /// </summary>
    public class GroupSignalsByApproaches : TransformManyProcessStepBase<Tuple<Signal, IEnumerable<ControllerEventLog>>, Tuple<Approach, IEnumerable<ControllerEventLog>>>
    {
        /// <inheritdoc/>
        public GroupSignalsByApproaches(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<Tuple<Approach, IEnumerable<ControllerEventLog>>>> Process(Tuple<Signal, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var signal = input.Item1;
            var logs = input.Item2;
            
            var result = signal.Approaches.Select(s => Tuple.Create(s, logs.FromSpecification(new ControllerLogSignalFilterSpecification(signal))));

            return Task.FromResult(result);
        }
    }
}
