using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class WorkflowHelpers
    {
        public static IReadOnlyList<CorrectedDetectorEvent> FilterCorrectedDetectorEvents(this Tuple<Approach, IEnumerable<CorrectedDetectorEvent>> input)
        {
            return FilterCorrectedDetectorEvents(input.Item1, input.Item2);
        }

        public static IReadOnlyList<CorrectedDetectorEvent> FilterCorrectedDetectorEvents(Approach approach, IEnumerable<CorrectedDetectorEvent> events)
        {
            return events?
                .Where(w => w.SignalIdentifier == approach?.Signal.SignalIdentifier && approach.Detectors
                .Select(s => s.DetectorChannel)
                .Contains(w.DetectorChannel))
                .ToList();
        }

        public static IReadOnlyList<Volumes> FilterCorrectedDetectorEvents(this Tuple<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>> input)
        {
            var test = input.Item2.Where(w => w.SignalIdentifier == input.Item1.Signal.SignalIdentifier)



            return input.Item2?
                .Where(w => w.SignalIdentifier == input.Item1?.Signal.SignalIdentifier && input.Item1.Detectors
                .Select(s => s.DetectorChannel)
                .Contains(w.DetectorChannel))
                .ToList();
        }
    }
}
