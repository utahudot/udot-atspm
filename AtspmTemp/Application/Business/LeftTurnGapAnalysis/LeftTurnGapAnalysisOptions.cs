﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.LeftTurnGapAnalysis/LeftTurnGapAnalysisOptions.cs
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

namespace Utah.Udot.Atspm.Business.LeftTurnGapAnalysis
{
    public class LeftTurnGapAnalysisOptions : OptionsBase
    {
        public const int EVENT_GREEN = 1;
        public const int EVENT_RED = 10;
        public const int EVENT_DET = 81;



        public LeftTurnGapAnalysisOptions()
        {
        }

        public double Gap1Min { get; set; } = 0;
        public double Gap1Max { get; set; } = 1;
        public double Gap2Min { get; set; } = 1;
        public double Gap2Max { get; set; } = 3.3;
        public double Gap3Min { get; set; } = 3.3;
        public double Gap3Max { get; set; } = 3.7;
        public double Gap4Min { get; set; } = 3.7;
        public double? Gap4Max { get; set; }
        public double? Gap5Min { get; set; }
        public double? Gap5Max { get; set; }
        public double? Gap6Min { get; set; }
        public double? Gap6Max { get; set; }
        public double? Gap7Min { get; set; }
        public double? Gap7Max { get; set; }
        public double? Gap8Min { get; set; }
        public double? Gap8Max { get; set; }
        public double? Gap9Min { get; set; }
        public double? Gap9Max { get; set; }
        public double? Gap10Min { get; set; }
        public double? Gap10Max { get; set; }
        public double? SumDurationGap1 { get; set; }
        public double? SumDurationGap2 { get; set; }
        public double? SumDurationGap3 { get; set; }
        public double TrendLineGapThreshold { get; set; } = 7.4;
        public int BinSize { get; set; }


    }
}
