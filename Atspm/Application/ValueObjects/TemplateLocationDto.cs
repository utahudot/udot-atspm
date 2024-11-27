#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models.ConfigurationModels.Dtos/TemplateLocationDto.cs
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

namespace Utah.Udot.Atspm.ValueObjects
{
    /// <summary>
    /// Location that data is being logged and monitored
    /// </summary>
    public class TemplateLocationDto
    {
        /// <summary>
        /// Latitude of location
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude of location
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Priamry name of location
        /// </summary>
        public string PrimaryName { get; set; }

        /// <summary>
        /// Secondary name of location
        /// </summary>
        public string SecondaryName { get; set; }

        /// <summary>
        /// Note
        /// </summary>
        public string? Note { get; set; }

        /// <inheritdoc/>
        public ICollection<Device> Devices { get; set; } = new HashSet<Device>();
    }


    public class TemplateLocationModifiedDto
    {
        public string Id { get; set; }
        /// <summary>
        /// Location
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// This is Protected or Permissive Phases that occurred in the logs but were not found in your current location.
        /// </summary>
        public List<short> LoggedButUnusedProtectedOrPermissivePhases { get; set; } = new List<short>();

        /// <summary>
        /// This is Overlap Phases that occurred in the logs but were not found in your current location.
        /// </summary>
        public List<short> LoggedButUnusedOverlapPhases { get; set; } = new List<short>();

        /// <summary>
        /// This is Pedestrian Phases that occurred in the logs but were not found in your current location.
        /// </summary>
        public List<short> LoggedButUnusedPedestrianPhases { get; set; } = new List<short>();

        /// <summary>
        /// This is Detector Channels that occurred in the logs but were not found in your current location.
        /// </summary>
        public List<short> LoggedButUnusedDetectorChannels { get; set; } = new List<short>();

        /// <summary>
        /// Approaches that were removed from the location
        /// </summary>
        public ICollection<Approach> RemovedApproaches { get; set; } = new HashSet<Approach>();
    }
}