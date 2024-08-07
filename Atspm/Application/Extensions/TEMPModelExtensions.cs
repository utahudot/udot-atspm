﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Extensions/TEMPModelExtensions.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Relationships;

namespace Utah.Udot.Atspm.Extensions
{
    public static class IRelatedDetectorsExtensions
    {
        public static IReadOnlyList<Detector> GetAllDetectorsOfDetectionType(this IRelatedDetectors item, DetectionType detectionType)
        {
            return item.Detectors.Where(w => w.DetectionTypes.Contains(detectionType)).ToList();
        }

        public static IRelatedDetectors SetDetChannelWhenMultipleDetectorsExist(this IRelatedDetectors item)
        {
            var detChannel = item.Detectors.ToList()[0].DetectorChannel + 1;

            foreach (var detector in item.Detectors)
            {
                detector.DetectorChannel = detChannel;
                if (item is IRelatedLocation s)
                {
                    detector.DectectorIdentifier = $"{s.Location.LocationIdentifier} {detector.DetectorChannel}";
                }
            }

            return item;
        }

        public static IReadOnlyList<Detector> GetDetectorsForMetricType(this IRelatedDetectors item, int metricTypeId)
        {
            return item.Detectors.Where(d => d.SupportsMetricType(metricTypeId)).ToList();
        }
    }

    public static class IRelatedDetectionTypesExtensions
    {
        public static bool SupportsMetricType(this IRelatedDetectionTypes item, int metricTypeId)
        {
            return item.DetectionTypes.Any(m => m.MeasureTypes.Any(a => a.Id == metricTypeId));
        }
    }

    public static class DetectorExtensions
    {
        /// <summary>
        /// Checks to see if <see cref="Detector"/>.<see cref="DetectionType"/> metrics contains <paramref name="metricId"/>
        /// </summary>
        /// <param name="detector"><see cref="Detector"/> whos <see cref="DetectionType"/> collection to check</param>
        /// <param name="metricId"><see cref="MeasureType"/> id to match within <see cref="DetectionType"/> collection</param>
        /// <returns></returns>
        public static bool CheckReportAvialbility(this Detector detector, int metricId)
        {
            return detector.DetectionTypes.SelectMany(s => s.MeasureTypes).Any(a => a.Id == metricId);
        }

        public static double GetOffset(this Detector detector)
        {
            detector.DecisionPoint ??= 0;

            if (detector.Approach.Mph.HasValue && detector.Approach.Mph > 0)
            {
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
            return item.GetDetectors().FirstOrDefault(f => f.DetectorChannel == channel);
        }

        public static IReadOnlyList<Detector> GetDetectors(this IRelatedApproaches item)
        {
            return item.Approaches.SelectMany(s => s.Detectors).OrderBy(o => o.DectectorIdentifier).ToList();
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

        public static IReadOnlyList<MeasureType> GetAvailableMetrics(this IRelatedApproaches item)
        {
            return item.GetDetectors().SelectMany(s => s.DetectionTypes).Where(d => d.Id != DetectionTypes.B).SelectMany(m => m.MeasureTypes).ToList();
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
