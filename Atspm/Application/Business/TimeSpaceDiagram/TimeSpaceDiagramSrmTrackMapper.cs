#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeSpaceDiagram/TimeSpaceDiagramSrmTrackMapper.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Business.TimeSpaceDiagram
{
    public static class TimeSpaceDiagramSrmTrackMapper
    {
        public static List<SrmEntityTrack> GetTracksForPhase(
            RouteLocation routeLocation,
            List<SrmEntityTrack> srmTracks,
            string phaseType,
            bool isFirstElement,
            bool isLastElement)
        {
            if (routeLocation == null || srmTracks == null || srmTracks.Count == 0)
            {
                return new List<SrmEntityTrack>();
            }

            if (
                (string.Equals(phaseType, "Primary", StringComparison.OrdinalIgnoreCase) &&
                 isLastElement) ||
                (string.Equals(phaseType, "Opposing", StringComparison.OrdinalIgnoreCase) &&
                 isFirstElement)
            )
            {
                return new List<SrmEntityTrack>();
            }

            var allForLocation = FilterTracksForLocation(
                srmTracks,
                routeLocation.LocationIdentifier);

            var targetDirection =
                string.Equals(phaseType, "Opposing", StringComparison.OrdinalIgnoreCase)
                    ? routeLocation.OpposingDirectionId
                    : routeLocation.PrimaryDirectionId;

            return allForLocation
                .Where(t => IsDirectionMatch(t.HeadingDirection, targetDirection))
                .ToList();
        }

        private static List<SrmEntityTrack> FilterTracksForLocation(
            List<SrmEntityTrack> tracks,
            string locationIdentifier)
        {
            if (tracks.Count == 0 || string.IsNullOrWhiteSpace(locationIdentifier))
            {
                return new List<SrmEntityTrack>();
            }

            return tracks
                .Where(t => string.Equals(
                    t.StartingIntersection,
                    locationIdentifier,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private static bool IsDirectionMatch(DirectionTypes heading, DirectionTypes target)
        {
            if (heading == DirectionTypes.NA || target == DirectionTypes.NA)
            {
                return false;
            }

            if (heading == target)
            {
                return true;
            }

            var headingDegrees = DirectionToDegrees(heading);
            var targetDegrees = DirectionToDegrees(target);
            if (!headingDegrees.HasValue || !targetDegrees.HasValue)
            {
                return false;
            }

            var diff = Math.Abs(headingDegrees.Value - targetDegrees.Value);
            var circularDiff = Math.Min(diff, 360 - diff);
            return circularDiff <= 45.0;
        }

        private static double? DirectionToDegrees(DirectionTypes direction)
        {
            return direction switch
            {
                DirectionTypes.NB => 0,
                DirectionTypes.NE => 45,
                DirectionTypes.EB => 90,
                DirectionTypes.SE => 135,
                DirectionTypes.SB => 180,
                DirectionTypes.SW => 225,
                DirectionTypes.WB => 270,
                DirectionTypes.NW => 315,
                _ => null
            };
        }
    }
}
