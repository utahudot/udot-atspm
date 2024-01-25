using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Domain.Common;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class AggregatePhaseTerminationEvents : TransformProcessStepBase<Tuple<Approach, int, PhaseTerminations>, IEnumerable<PhaseTerminationAggregation>>
    {
        /// <inheritdoc/>
        public AggregatePhaseTerminationEvents(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<PhaseTerminationAggregation>> Process(Tuple<Approach, int, PhaseTerminations> input, CancellationToken cancelToken = default)
        {
            var approach = input.Item1;
            var phase = input.Item2;
            var events = input.Item3;

            var tl = new Timeline<PhaseTerminationAggregation>(events.TerminationEvents, TimeSpan.FromMinutes(15));

            tl.Segments.ToList().ForEach(f =>
            {
                f.LocationIdentifier = approach.Location.LocationIdentifier;
                f.PhaseNumber = phase;
                f.GapOuts = events.GapOuts.Count(c => f.InRange(c));
                f.ForceOffs = events.ForceOffs.Count(c => f.InRange(c));
                f.MaxOuts = events.MaxOuts.Count(c => f.InRange(c));
                f.Unknown = events.UnknownTerminations.Count(c => f.InRange(c));
            });

            //TODO: change this and all aggregations to ireadonlylist
            var result = tl.Segments.AsEnumerable();

            return Task.FromResult(result);
        }
    }
}
