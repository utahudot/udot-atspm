#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeSpaceDiagram/MergingApproaches.cs
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

using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Business.TimeSpaceDiagram
{
    public class MergingApproaches
    {
        public DirectionTypes RightTurnFrom { get; set; }
        public DirectionTypes LeftTurnFrom { get; set; }
    }

    public class TmcEventDto : DataPointDateDouble
    {
        public TmcEventDto(DateTime start, double value) : base(start, value) { }

        public bool IsRightTurnEvent { get; set; }
        public bool IsLeftTurnEvent { get; set; }
        public LaneTypes LaneType { get; set; }
        public DirectionTypes DirectionType { get; set; }
    }

    public class TmcForPhaseDto
    {
        public List<TmcEventDto> RightTurnEvents { get; set; } = new List<TmcEventDto>();
        public List<TmcEventDto> LeftTurnEvents { get; set; } = new List<TmcEventDto>();
    }
}
