#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/DeviceDownloaderConfiguration.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Options pattern model for services that implement <see cref="IDeviceDownloader"/>
    /// </summary>
    public class DeviceDownloaderConfiguration
    {
        /// <summary>
        /// Base path to store downloaded event logs
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Flag for deleting remote file after downloading
        /// </summary>
        public bool DeleteRemoteFile { get; set; }

        /// <summary>
        /// Flag to ping <see cref="Device"/> to verify <see cref="Device.Ipaddress"/> before downloading
        /// </summary>
        public bool Ping { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{BasePath} - {DeleteRemoteFile} - {Ping}";
        }
    }
}
