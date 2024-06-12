#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.ApproachVolume/ApproachVolumeResult.cs
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
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ATSPM.Application.Analysis.ApproachVolume
{
    public class ApproachVolumeResult
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public int PrimaryTotalVolume { get; set; }
        public int PrimaryPeakVolume { get; set; }
        public double PrimaryPHF { get; set; }
        public double PrimaryDFactor { get; set; }
        public double PrimaryKFactor { get; set; }

        public int OpposingTotalVolume { get; set; }
        public int OpposingPeakVolume { get; set; }
        public double OpposingPHF { get; set; }
        public double OpposingDFactor { get; set; }
        public double OpposingKFactor { get; set; }

        public int TotalVolume { get; set; }
        public int TotalPeakVolume { get; set; }
        public double TotalPHF { get; set; }
        public double TotalKFactor { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
