using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;

namespace ATSPM.Application.Analysis.Common
{
    public class CorrectedDetectorEvent
    {
        public CorrectedDetectorEvent(Detector detector)
        {
            Detector = detector;
            //CorrectedTimeStamp = AtspmMath.AdjustTimeStamp(timeStamp,
            //                                              Detector.Approach?.Mph ?? 0,
            //                                               Detector.DistanceFromStopBar ?? 0,
            //                                               Detector.LatencyCorrection);
        }
        public DateTime CorrectedTimeStamp { get; set; }

        public Detector Detector { get; internal set; }

        public override string ToString()
        {
            return $"{Detector.Approach.Signal.SignalIdentifier}-{Detector.DetectorChannel}-{CorrectedTimeStamp:yyyy-MM-dd'T'HH:mm:ss.f}";
        }
    }
}
