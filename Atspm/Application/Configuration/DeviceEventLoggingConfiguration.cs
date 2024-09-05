#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Configuration/DeviceEventLoggingConfiguration.cs
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

namespace Utah.Udot.Atspm.Configuration
{
    /// <summary>
    /// Configuration options for device event logging
    /// </summary>
    public class DeviceEventLoggingConfiguration
    {
        /// <summary>
        /// Path to logging directory
        /// </summary>
        public string Path { get; set; } = System.IO.Path.GetTempPath();

        /// <inheritdoc cref="DeviceEventLoggingQueryOptions"/>
        public DeviceEventLoggingQueryOptions DeviceEventLoggingQueryOptions { get; set; } = new DeviceEventLoggingQueryOptions();

        public override string ToString()
        {
            return $"{DeviceEventLoggingQueryOptions} --- {Path}";
        }
    }

    /// <summary>
    /// Options for querying which <see cref="Device"/> to log.
    /// </summary>
    public class DeviceEventLoggingQueryOptions
    {
        /// <summary>
        /// List of <see cref="Location.LocationIdentifier"/> to include
        /// </summary>
        public IEnumerable<string> IncludedLocations { get; set; }

        /// <summary>
        /// List of <see cref="Location.LocationIdentifier"/> to exclude
        /// </summary>
        public IEnumerable<string> ExcludedLocations { get; set; }

        /// <summary>
        /// List of <see cref="Area.Name"/> to include
        /// </summary>
        public IEnumerable<string> IncludedAreas { get; set; }

        /// <summary>
        /// List of <see cref="Jurisdiction.Name"/> to include
        /// </summary>
        public IEnumerable<string> IncludedJurisdictions { get; set; }

        /// <summary>
        /// List of <see cref="Region.Description"/> to include
        /// </summary>
        public IEnumerable<string> IncludedRegions { get; set; }

        /// <summary>
        /// List of <see cref="LocationType.Name"/> to include
        /// </summary>
        public IEnumerable<string> IncludedLocationTypes { get; set; }

        /// <summary>
        /// <see cref="DeviceTypes"/> to include
        /// </summary>
        public DeviceTypes DeviceType { get; set; } = DeviceTypes.Unknown;

        /// <summary>
        /// <see cref="TransportProtocols"/> to include
        /// </summary>
        public TransportProtocols TransportProtocol { get; set; }

        public override string ToString()
        {
            return $"{DeviceType} - {TransportProtocol}";
        }
    }
}
