#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Enums/WatchDogIssueTypes.cs
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

namespace Utah.Udot.Atspm.Data.Enums
{
    /// <summary>
    /// Watchdog issue types
    /// </summary>
    public enum WatchDogIssueTypes
    {
        /// <summary>
        /// Record count
        /// </summary>
        RecordCount = 1,

        /// <summary>
        /// Low detector hits
        /// </summary>
        LowDetectorHits = 2,

        /// <summary>
        /// Stuck ped
        /// </summary>
        StuckPed = 3,

        /// <summary>
        /// Forceoff threshold
        /// </summary>
        ForceOffThreshold = 4,

        /// <summary>
        /// Max out threshold
        /// </summary>
        MaxOutThreshold = 5,

        /// <summary>
        /// Unconfigured approach
        /// </summary>
        UnconfiguredApproach = 6,

        /// <summary>
        /// Unconfigured detector
        /// </summary>
        UnconfiguredDetector = 7,

        /// <summary>
        /// Missing Mainline Data
        /// </summary>
        MissingMainlineData = 8,

        /// <summary>
        /// Stuck Queue Detection
        /// </summary>
        StuckQueueDetection = 9,
    }
}