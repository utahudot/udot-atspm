#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.ATSPM.ConfigApi.DTO/DetectionTypeDto.cs
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

namespace Utah.Udot.ATSPM.ConfigApi.DTO
{
    public class DetectionTypeDto
    {
        public DetectionTypes Id { get; set; }
        public string Description { get; set; }
        public string Abbreviation { get; set; }
        public int DisplayOrder { get; set; }
        public ICollection<MeasureTypeDto> MeasureTypes { get; set; }
    }

    public class MeasureTypeDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public bool ShowOnWebsite { get; set; }
        public bool ShowOnAggregationSite { get; set; }
        public int DisplayOrder { get; set; }
        public virtual ICollection<MeasureCommentsDto> MeasureComments { get; set; } = new HashSet<MeasureCommentsDto>();
        public virtual ICollection<MeasureOptionDto> MeasureOptions { get; set; } = new HashSet<MeasureOptionDto>();

    }

    public class MeasureCommentsDto
    {
        public int? Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Comment { get; set; }
        public string LocationIdentifier { get; set; }

    }

    public class MeasureOptionDto
    {
        public int? Id { get; set; }
        public string Option { get; set; }
        public string Value { get; set; }
        public int MeasureTypeId { get; set; }
    }
}
