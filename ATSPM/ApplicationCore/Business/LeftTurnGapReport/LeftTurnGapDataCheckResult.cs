#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.LeftTurnGapReport/LeftTurnGapDataCheckResult.cs
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
using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class LeftTurnGapDataCheckResult : ApproachResult
    {
        public bool LeftTurnVolumeOk { get; set; }
        public bool GapOutOk { get; set; }
        public bool PedCycleOk { get; set; }
        public bool InsufficientDetectorEventCount { get; set; }
        public bool InsufficientCycleAggregation { get; set; }
        public bool InsufficientPhaseTermination { get; set; }
        public bool InsufficientPedAggregations { get; set; }
        public bool InsufficientSplitFailAggregations { get; set; }
        public bool InsufficientLeftTurnGapAggregations { get; set; }
    }
}
