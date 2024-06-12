#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.TimeSpaceDiagram/TimeSpaceDiagramAverageOptions.cs
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
using System.Collections.Generic;

namespace ATSPM.Application.Business.TimeSpaceDiagram
{
    public class TimeSpaceDiagramAverageOptions
    {
        public int RouteId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public List<LocationWithSequence> Sequence { get; set; }
        public List<LocationWithCoordPhases> CoordinatedPhases { get; set; }
        public int[] DaysOfWeek { get; set; }
        public int? SpeedLimit { get; set; }
    }
}
