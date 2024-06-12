#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Watchdog/WatchDogLogEventDTO.cs
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
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.Watchdog
{
    public class WatchDogLogEventDTO : WatchDogLogEvent
    {
        public WatchDogLogEventDTO(
            int locationId,
            string locationIdentifier,
            DateTime timestamp,
            WatchDogComponentType componentType,
            int componentId,
            WatchDogIssueType issueType,
            string details,
            int? phase,
            int? regionId,
            string? regionDescription,
            int? jurisdictionId,
            string? jurisdictionName,
            IEnumerable<AreaDTO> areas) : base(locationId, locationIdentifier, timestamp, componentType, componentId, issueType, details, phase)
        {
            RegionId = regionId;
            RegionDescription = regionDescription;
            JurisdictionId = jurisdictionId;
            JurisdictionName = jurisdictionName;
            Areas = areas;
        }

        public int? RegionId { get; set; }
        public string? RegionDescription { get; set; }
        public int? JurisdictionId { get; set; }
        public string? JurisdictionName { get; set; }
        public IEnumerable<AreaDTO> Areas { get; set; }

    }
}
