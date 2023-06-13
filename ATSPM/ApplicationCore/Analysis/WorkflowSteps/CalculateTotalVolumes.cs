using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Workflows;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculateTotalVolumes : TransformManyProcessStepBase<IEnumerable<CorrectedDetectorEvent>, TotalVolumes>
    {
        private readonly TimelineOptions _options;

        public CalculateTotalVolumes(TimelineOptions options, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _options = options;
        }

        protected override Task<IEnumerable<TotalVolumes>> Process(IEnumerable<CorrectedDetectorEvent> input, CancellationToken cancelToken = default)
        {
            var result = new List<TotalVolumes>();

            var test = input.GroupBy(g => g.Detector.Approach);

            foreach (var t in test)
            {
                var total = new TotalVolumes(_options);

                var c = new OpposingDirection(t.Key.DirectionTypeId);

                var o = test.Where(w => w.Key.DirectionTypeId == c).FirstOrDefault();

                total.ForEach(f =>
                {
                    f.Primary = new Volume()
                    {
                        Start = f.Start,
                        End = f.End,
                        Direction = t.Key.DirectionTypeId,
                        Phase = t.Key.ProtectedPhaseNumber,
                        DetectorCount = t.Where(w => f.InRange(w.CorrectedTimeStamp)).Count()
                    };

                    f.Opposing = new Volume()
                    {
                        Start = f.Start,
                        End = f.End,
                        Direction = o.Key.DirectionTypeId,
                        Phase = o.Key.ProtectedPhaseNumber,
                        DetectorCount = o.Where(w => f.InRange(w.CorrectedTimeStamp)).Count()
                    };
                });

                result.Add(total);

                foreach (var a in total)
                {
                    Console.WriteLine($"p: {a.Primary}");
                    Console.WriteLine($"o: {a.Opposing}");
                }
            }

            return Task.FromResult<IEnumerable<TotalVolumes>>(result);
        }
    }
}
