#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.ApproachVolume/ApproachVolumeOptions.cs
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

namespace ATSPM.Application.Business.ApproachVolume
{
    public class ApproachVolumeOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public bool ShowDirectionalSplits { get; set; }
        public bool GetVolume { get; set; } = true;
        public bool ShowNbEbVolume { get; set; }
        public bool ShowSbWbVolume { get; set; }
        public bool ShowTMCDetection { get; set; }
        public bool ShowAdvanceDetection { get; set; }
        public int MetricTypeId { get; internal set; } = 7;
    }
}