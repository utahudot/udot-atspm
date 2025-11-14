#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/DeviceEventLoggingConfiguration.cs
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

using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options for device event logging
    /// </summary>
    public class DeviceEventLoggingConfiguration
    {
        /// <summary>
        /// Path to local directory where event logs are saved
        /// </summary>
        public string Path { get; set; } = System.IO.Path.GetTempPath();

        /// <summary>
        /// Batch size of <see cref="EventLogModelBase"/> objects when saving to repository
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Amount of processes that can be run in parallel
        /// </summary>
        public int ParallelProcesses { get; set; }

        /// <inheritdoc cref="DeviceEventLoggingQueryOptions"/>
        public DeviceEventLoggingQueryOptions DeviceEventLoggingQueryOptions { get; set; } = new();
    }
}
