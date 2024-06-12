#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.LinkPivot/LinkPivotPcdOptions.cs
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
using System;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotPcdOptions
    {
        public string LocationIdentifier { get; set; }
        public string DownstreamLocationIdentifier { get; set; }
        [Required]
        public int Delta { get; set; }
        public string DownstreamApproachDirection { get; set; }
        public string UpstreamApproachDirection { get; set; }
        public DateOnly StartDate { get; set; }
        public DateTime? SelectedEndDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
