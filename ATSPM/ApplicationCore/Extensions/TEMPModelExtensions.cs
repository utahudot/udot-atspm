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
        public static IReadOnlyList<Detector> GetAllDetectorsOfDetectionType(this IRelatedDetectors item, DetectionType detectionType)
        {
            return item.Detectors.Where(w => w.DetectionTypes.Contains(detectionType)).ToList();
        }

        public static IRelatedDetectors SetDetChannelWhenMultipleDetectorsExist(this IRelatedDetectors item)
        {
            var detChannel = item.Detectors.ToList()[0].DetChannel + 1;

            foreach(var detector in item.Detectors)
            {
                detector.DetChannel = detChannel;
                if (item is IRelatedSignal s)
                {
                    detector.DetectorId = $"{s.Signal.SignalIdentifier} {detector.DetChannel}";
                }
            }

            return item;
        }

        public static IReadOnlyList<Detector> GetDetectorsForMetricType(this IRelatedDetectors item, int metricTypeId)
        {
            //TODO: parts of this are resuable, consider moving to specification or extension method
            return item.Detectors.Where(d => d.SupportsMetricType(metricTypeId)).ToList();
        }
    }

    public static class IRelatedDetectionTypesExtensions
    {
        public static bool SupportsMetricType(this IRelatedDetectionTypes item, int metricTypeId)
        {
            return item.DetectionTypes.Any(m => m.MetricTypeMetrics.Any(a => a.Id == metricTypeId));
        }
    }

    public static class DetectorExtensions
    {
        /// <summary>
        /// Checks to see if <see cref="Detector"/>.<see cref="DetectionType"/> metrics contains <paramref name="metricId"/>
        /// </summary>
        /// <param name="detector"><see cref="Detector"/> whos <see cref="DetectionType"/> collection to check</param>
        /// <param name="metricId"><see cref="MetricType"/> id to match within <see cref="DetectionType"/> collection</param>
        /// <returns></returns>
        public static bool CheckReportAvialbility (this Detector detector, int metricId)
        {
            return detector.DetectionTypes.SelectMany(s => s.MetricTypeMetrics).Any(a => a.Id == metricId);
        }

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

    public static class IRelatedApproachesExtensions
    {
        public static IReadOnlyList<Approach> GetApproaches(this IRelatedApproaches item, int metricTypeID)
        {
            return item.Approaches.Where(a => a.Detectors.Any(d => d.SupportsMetricType(metricTypeID)))
                .OrderBy(o => o.PermissivePhaseNumber)
                .ThenBy(o => o.ProtectedPhaseNumber)
                .ThenBy(o => o.DirectionTypeId.ToString())
                .ToList();
        }

        public static IReadOnlyList<DirectionTypes> GetAvailableDirections(this IRelatedApproaches item)
        {
            return item.Approaches.Select(s => s.DirectionTypeId).Distinct().ToList();
        }

        public static Detector GetDetector(this IRelatedApproaches item, int channel)
        {
            return item.GetDetectors().FirstOrDefault(f => f.DetChannel == channel);
        }

        public static IReadOnlyList<Detector> GetDetectors(this IRelatedApproaches item)
        {
            return item.Approaches.SelectMany(s => s.Detectors).OrderBy(o => o.DetectorId).ToList();
        }

        public static IReadOnlyList<Detector> GetDetectors(this IRelatedApproaches item, int metricTypeId, DirectionTypes direction)
        {
            return item.Approaches.Where(w => w.DirectionTypeId == direction).SelectMany(s => s.Detectors).Where(d => d.CheckReportAvialbility(metricTypeId)).ToList();
        }

        public static IReadOnlyList<Detector> GetDetectors(this IRelatedApproaches item, int phase)
        {
            return item.Approaches.Where(w => w.ProtectedPhaseNumber == phase || w.PermissivePhaseNumber == phase).SelectMany(s => s.Detectors).ToList();
        }

        public static IReadOnlyList<Detector> GetDetectors(this IRelatedApproaches item, int metricTypeId, int phase)
        {
            return item.Approaches.Where(w => w.ProtectedPhaseNumber == phase || w.PermissivePhaseNumber == phase).SelectMany(s => s.Detectors).Where(d => d.CheckReportAvialbility(metricTypeId)).ToList();
        }

        public static IReadOnlyList<MetricType> GetAvailableMetrics(this IRelatedApproaches item)
        {
            return item.GetDetectors().SelectMany(s => s.DetectionTypes).Where(d => d.Id != DetectionTypes.B).SelectMany(m => m.MetricTypeMetrics).ToList();
        }

        public static IReadOnlyList<int> GetPhases(this IRelatedApproaches item)
        {
            var phases = new List<int>();

            foreach (var a in item.Approaches)
            {
                if (a.PermissivePhaseNumber != null)
                    phases.Add(a.PermissivePhaseNumber.Value);
                phases.Add(a.ProtectedPhaseNumber);
            }

            return phases.Select(p => p).Distinct().ToList();
        }
    }
}
