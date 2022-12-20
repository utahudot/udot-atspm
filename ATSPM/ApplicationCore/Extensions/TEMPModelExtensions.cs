using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Relationships;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class IRelatedDetectorsExtensions
    {
        public static IReadOnlyList<Detector> GetAllDetectorsOfDetectionType(this IRelatedDetectors model, DetectionType detectionType)
        {
            return model.Detectors.Where(w => w.DetectionTypes.Contains(detectionType)).ToList();
        }

        public static IRelatedDetectors SetDetChannelWhenMultipleDetectorsExist(this IRelatedDetectors model)
        {
            var detChannel = model.Detectors.ToList()[0].DetChannel + 1;

            foreach(var detector in model.Detectors)
            {
                detector.DetChannel = detChannel;
                if (model is IRelatedSignal s)
                {
                    detector.DetectorId = $"{s.Signal.SignalId} {detector.DetChannel}";
                }
            }

            return model;
        }

        public static IReadOnlyList<Detector> GetDetectorsForMetricType(this IRelatedDetectors model, int metricTypeId)
        {
            //TODO: parts of this are resuable, consider moving to specification or extension method
            return model.Detectors.Where(d => d.SupportsMetricType(metricTypeId)).ToList();
        }
    }

    public static class IRelatedDetectionTypesExtensions
    {
        public static bool SupportsMetricType(this IRelatedDetectionTypes model, int metricTypeId)
        {
            return model.DetectionTypes.Any(m => m.MetricTypeMetrics.Any(a => a.Id == metricTypeId));
        }
    }

    public static class DetectorExtensions
    {
        public static double GetOffset(this Detector detector)
        {
            detector.DecisionPoint ??= 0;

            if (detector.Approach.Mph.HasValue && detector.Approach.Mph > 0)
            {
                //TODO: see if this is duplicated anywhere else
                return Convert.ToDouble((detector.DistanceFromStopBar / (detector.Approach.Mph * 1.467) - detector.DecisionPoint) * 1000);
            }

            return 0;
        }
    }
}
