using ATSPM.Application.Analysis.ApproachVolume;
using ATSPM.Application.Analysis.Common;
using ATSPM.Domain.Workflows;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class GenerateApproachVolumeResults : TransformProcessStepBase<TotalVolumes, ApproachVolumeResult>
    {
        public GenerateApproachVolumeResults(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<ApproachVolumeResult> Process(TotalVolumes input, CancellationToken cancelToken = default)
        {
            var chunks = 60 / input.SegmentSpan.Minutes;

            var peakA = input.Segments.Select(s => s.Primary).GetPeakVolumes(chunks).ToList();
            var o = input.Segments.Where(w => peakA.Contains(w.Primary)).Select(s => s.Opposing).Sum(s => s.DetectorCount);

            var peakB = input.Segments.Select(s => s.Opposing).GetPeakVolumes(chunks);
            var p = input.Segments.Where(w => peakB.Contains(w.Opposing)).Select(s => s.Primary).Sum(s => s.DetectorCount);

            var peakTotal = input.Segments.GetPeakVolumes(chunks);

            var result = new ApproachVolumeResult();

            result.Start = input.Start;
            result.End = input.End;

            result.PrimaryTotalVolume = input.Segments.Select(s => s.Primary).Sum(s => s.DetectorCount);
            result.PrimaryPeakVolume = peakA.Sum(s => s.DetectorCount);
            result.PrimaryPHF = AtspmMath.PeakHourFactor(result.PrimaryPeakVolume, peakA.Max(m => m.DetectorCount), chunks);

            result.OpposingTotalVolume = input.Segments.Select(s => s.Opposing).Sum(s => s.DetectorCount);
            result.OpposingPeakVolume = peakB.Sum(s => s.DetectorCount);
            result.OpposingPHF = AtspmMath.PeakHourFactor(result.OpposingPeakVolume, peakB.Max(m => m.DetectorCount), chunks);

            result.PrimaryDFactor = AtspmMath.PeakHourDFactor(result.PrimaryPeakVolume, o);
            result.OpposingDFactor = AtspmMath.PeakHourDFactor(result.OpposingPeakVolume, p);
            result.PrimaryKFactor = AtspmMath.PeakHourKFactor(result.PrimaryTotalVolume, result.PrimaryPeakVolume, result.OpposingTotalVolume, o);
            result.OpposingKFactor = AtspmMath.PeakHourKFactor(result.OpposingTotalVolume, result.OpposingPeakVolume, result.PrimaryTotalVolume, p);

            result.TotalVolume = input.DetectorCount;
            result.TotalPeakVolume = peakTotal.Sum(s => s.DetectorCount);
            result.TotalPHF = AtspmMath.PeakHourFactor(result.TotalPeakVolume, peakTotal.Max(m => m.DetectorCount), chunks);
            result.TotalKFactor = Math.Round(Convert.ToDouble(result.TotalPeakVolume) / Convert.ToDouble(result.TotalVolume), 3);

            return Task.FromResult(result);
        }
    }
}
