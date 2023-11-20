using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Common;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Workflows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class TestCalculateTotalVolumes : TransformProcessStepBase<Tuple<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>>, Tuple<Approach, TotalVolumes>>
    {
        protected override Task<Tuple<Approach, TotalVolumes>> Process(Tuple<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>> input, CancellationToken cancelToken = default)
        {
            var result = new TotalVolumes(new TimelineOptions()
            {
                Start = DateTime.Parse("4/17/2023 8:00:00"),
                End = DateTime.Parse("4/17/2023 10:00:00"),
                Type = TimelineType.Minutes,
                Size = 15
            });

            if (input.Item1.Item1.DirectionTypeId == new OpposingDirection(input.Item2.Item1.DirectionTypeId))
            {
                result.ForEach(f =>
                {
                    f.Primary = input.Item1.Item2.FirstOrDefault(d => d.Start == f.Start && d.End == f.End);
                    f.Opposing = input.Item2.Item2.FirstOrDefault(d => d.Start == f.Start && d.End == f.End);
                });
            }

            return Task.FromResult(Tuple.Create(input.Item1.Item1, result));

        }
    }

    public class CalculateTotalVolumes : TransformProcessStepBase<Tuple<Approach, IEnumerable<Vehicle>>, TotalVolumes>
    {
        public CalculateTotalVolumes(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<TotalVolumes> Process(Tuple<Approach, IEnumerable<Vehicle>> input, CancellationToken cancelToken = default)
        {
            //var primary = input.Item1;
            //var opposing = primary.Signal.Approaches.Where(w => w.DirectionTypeId == new OpposingDirection(primary.DirectionTypeId));

            var result = new TotalVolumes(new TimelineOptions()
            {
                Start = input.Item2.Min(m => m.CorrectedTimeStamp),
                End = input.Item2.Max(m => m.CorrectedTimeStamp),
                Type = TimelineType.Minutes,
                Size = 15
            });

            //var o = input.Item1.Detectors.Where(w => w.Item1.Approach.DirectionTypeId == new OpposingDirection(t.Item1.Approach.DirectionTypeId)).FirstOrDefault();

            //result.ForEach(f =>
            //{
            //    f.Primary = new Volume()
            //    {
            //        Start = f.Start,
            //        End = f.End,
            //        Direction = t.Item1.Approach.DirectionTypeId,
            //        //TODO: get real phase number
            //        Phase = t.Item1.Approach.ProtectedPhaseNumber,
            //        DetectorCount = t.Item2.Where(w => f.InRange(w.CorrectedTimeStamp)).Count()
            //    };

            //    f.Opposing = new Volume()
            //    {
            //        Start = f.Start,
            //        End = f.End,
            //        Direction = o.Item1.Approach.DirectionTypeId,
            //        //TODO: get real phase number
            //        Phase = o.Item1.Approach.ProtectedPhaseNumber,
            //        DetectorCount = o.Item2.Where(w => f.InRange(w.CorrectedTimeStamp)).Count()
            //    };
            //});




            //var result = new TotalVolumes(new TimelineOptions()
            //{
            //    Start = input.SelectMany(m => m.Item2).Min(m => m.CorrectedTimeStamp),
            //    End = input.SelectMany(m => m.Item2).Max(m => m.CorrectedTimeStamp),
            //    Type = TimelineType.Minutes,
            //    Size = 15
            //});

            //foreach (var t in input)
            //{
            //    var o = input.Where(w => w.Item1.Approach.DirectionTypeId == new OpposingDirection(t.Item1.Approach.DirectionTypeId)).FirstOrDefault();

            //    result.ForEach(f =>
            //    {
            //        f.Primary = new Volume()
            //        {
            //            Start = f.Start,
            //            End = f.End,
            //            Direction = t.Item1.Approach.DirectionTypeId,
            //            //TODO: get real phase number
            //            Phase = t.Item1.Approach.ProtectedPhaseNumber,
            //            DetectorCount = t.Item2.Where(w => f.InRange(w.CorrectedTimeStamp)).Count()
            //        };

            //        f.Opposing = new Volume()
            //        {
            //            Start = f.Start,
            //            End = f.End,
            //            Direction = o.Item1.Approach.DirectionTypeId,
            //            //TODO: get real phase number
            //            Phase = o.Item1.Approach.ProtectedPhaseNumber,
            //            DetectorCount = o.Item2.Where(w => f.InRange(w.CorrectedTimeStamp)).Count()
            //        };
            //    });
            //}

            return Task.FromResult<TotalVolumes>(result);
        }
    }
}
