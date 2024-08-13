#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Configuration/DeviceDownloaderConfiguration.cs
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

using Utah.Udot.Atspm.Services;

namespace Utah.Udot.Atspm.Configuration
{
    /// <summary>
    /// Options pattern model for services that implement <see cref="IDeviceDownloader"/>
    /// </summary>
    public class DeviceDownloaderConfiguration
    {
        /// <summary>
        /// Local path to store downloaded event logs
        /// </summary>
        public string LocalPath { get; set; }

        /// <summary>
        /// Flag for deleting remote file after downloading
        /// </summary>
        public bool DeleteFile { get; set; }

        /// <summary>
        /// Flag to ping <see cref="Device"/> to verify <see cref="Device.Ipaddress"/>
        /// </summary>
        public bool Ping { get; set; }
    }
}
