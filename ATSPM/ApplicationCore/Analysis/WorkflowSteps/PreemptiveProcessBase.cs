using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public abstract class PreemptiveProcessBase<T> : TransformManyProcessStepBase<Tuple<Location, IEnumerable<ControllerEventLog>, int>, T> where T : PreempDetailValueBase, new()
    {
        protected IndianaEnumerations first;
        protected IndianaEnumerations second;

        public PreemptiveProcessBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<T>> Process(Tuple<Location, IEnumerable<ControllerEventLog>, int> input, CancellationToken cancelToken = default)
        {
            //var result = Tuple.Create(input.Item1, input.Item2
            //    .Where(w => w.locationIdentifier == input.Item1.locationIdentifier)
            //    .Where(w => w.EventParam == input.Item3)
            //    .TimeSpanFromConsecutiveCodes(first, second)
            //    .Select(s => new T()
            //    {
            //        locationIdentifier = input.Item1.locationIdentifier,
            //        PreemptNumber = input.Item3,
            //        Start = s.Item1[0].Timestamp,
            //        End = s.Item1[1].Timestamp,
            //        Seconds = s.Item2
            //    }), input.Item3);

            var result = input.Item2
                .Where(w => w.SignalIdentifier == input.Item1.LocationIdentifier)
                .Where(w => w.EventParam == input.Item3)
                .TimeSpanFromConsecutiveCodes(first, second)
                .Select(s => new T()
                {
                    LocationIdentifier = input.Item1.LocationIdentifier,
                    PreemptNumber = input.Item3,
                    Start = s.Item1[0].Timestamp,
                    End = s.Item1[1].Timestamp,
                    Seconds = s.Item2
                });

            return Task.FromResult(result);
        }
    }

    //public abstract class PreemptiveProcessBase<T> : TransformManyProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<T>> where T : PreempDetailValueBase, new()
    //{
    //    protected IndianaEnumerations first;
    //    protected IndianaEnumerations second;

    //    public PreemptiveProcessBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

    //    protected override Task<IEnumerable<IEnumerable<T>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
    //    {
    //        var result = input.GroupBy(g => g.locationIdentifier)
    //            .SelectMany(s => s.GroupBy(g => g.EventParam)
    //            .Select(s => s.TimeSpanFromConsecutiveCodes(first, second)
    //            .Select(s => new T()
    //            {
    //                locationIdentifier = s.Item1[0].locationIdentifier == s.Item1[1].locationIdentifier ? s.Item1[0].locationIdentifier : string.Empty,
    //                PreemptNumber = Convert.ToInt32(s.Item1.Average(a => a.EventParam)),
    //                Start = s.Item1[0].Timestamp,
    //                End = s.Item1[1].Timestamp,
    //                Seconds = s.Item2
    //            })));

    //        return Task.FromResult(result);
    //    }
    //}
}
