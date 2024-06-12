#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.TempExtensions/LocationExtensions.cs
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
using ATSPM.Application.Extensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.TempExtensions
{
    public static class LocationExtensions
    {
        public static string LocationDescription(this Location Location)
        {
            return $"#{Location.LocationIdentifier} - {Location.PrimaryName} & {Location.SecondaryName}";
        }

        public static List<Approach> GetApproachesForLocationThatSupportMetric(this Location Location, int metricTypeID)
        {
            var approachesForMeticType = new List<Approach>();
            foreach (var a in Location.Approaches)
                foreach (var d in a.Detectors)
                    if (d.SupportsMetricType(metricTypeID))
                    {
                        approachesForMeticType.Add(a);
                        break;
                    }
            //return approachesForMeticType;
            return approachesForMeticType.OrderBy(a => a.PermissivePhaseNumber).ThenBy(a => a.ProtectedPhaseNumber).ThenBy(a => a.DirectionType.Description)
                .ToList();
        }

        public static List<Detector> GetDetectorsForLocationThatSupportMetric(this Location Location, int metricTypeID)
        {
            var detectorsForMetricType = new List<Detector>();
            foreach (var a in Location.Approaches)
                foreach (var d in a.Detectors)
                    if (d.SupportsMetricType(metricTypeID))
                    {
                        detectorsForMetricType.Add(d);
                        break;
                    }
            //return approachesForMeticType;
            return detectorsForMetricType;
        }

        public static List<Detector> GetDetectorsForSignalThatSupportAMetricByApproachDirection(this Location location, int metricTypeId,
            DirectionTypes direction)
        {
            var detectorsForMetricType = new List<Detector>();
            foreach (var a in location.Approaches.Where(a => a.DirectionTypeId == direction))
                foreach (var d in a.Detectors)
                    if (d.SupportsMetricType(metricTypeId))
                    {
                        detectorsForMetricType.Add(d);
                        break;
                    }
            return detectorsForMetricType;
        }

        public static List<Detector> GetDetectorsForLocation(this Location Location)
        {
            var detectors = new List<Detector>();
            if (Location.Approaches != null)
            {
                foreach (var a in Location.Approaches.OrderBy(a => a.ProtectedPhaseNumber))
                    foreach (var d in a.Detectors)
                        detectors.Add(d);
            }

            return detectors.OrderBy(d => d.Id).ToList();
        }
        public static List<DirectionTypes> GetAvailableDirections(this Location Location)
        {
            var directions = Location.Approaches.Select(a => a.DirectionTypeId).Distinct().ToList();
            return directions;
        }

        public static List<int> GetPhasesForSignal(this Location Location)
        {
            var phases = new List<int>();
            foreach (var a in Location.Approaches)
            {
                if (a.PermissivePhaseNumber != null)
                    phases.Add(a.PermissivePhaseNumber.Value);
                phases.Add(a.ProtectedPhaseNumber);
            }
            return phases.Select(p => p).Distinct().ToList();
        }
    }
}
