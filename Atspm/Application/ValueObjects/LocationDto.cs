#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Models/ApproachDto.cs
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


#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Models/ApproachDto.cs
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

namespace Utah.Udot.Atspm.ValueObjects
{
    public class LocationDto
    {
        public int? Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public bool ChartEnabled { get; set; }
        public LocationVersionActions VersionAction { get; set; }
        public string Note { get; set; }
        public DateTime Start { get; set; }
        public bool PedsAre1to1 { get; set; }
        public string LocationIdentifier { get; set; }
        public int? JurisdictionId { get; set; }
        public JurisdictionDto Jurisdiction { get; set; }
        public int LocationTypeId { get; set; }
        public LocationTypeDto LocationType { get; set; }
        public int? RegionId { get; set; }
        public RegionDto Region { get; set; }
        public ICollection<ApproachDto> Approaches { get; set; } = new HashSet<ApproachDto>();
        public ICollection<AreaDto> Areas { get; set; } = new HashSet<AreaDto>();
        public ICollection<DeviceDto> Devices { get; set; } = new HashSet<DeviceDto>();
    }
}
