#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.SplitFail/SplitFailBin.cs
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.SplitFail
{
    public class SplitFailBin
    {
        public SplitFailBin(DateTime startTime, DateTime endTime, List<CycleSplitFail> cycles)
        {
            StartTime = startTime;
            EndTime = endTime;
            if (cycles.Count > 0)
            {
                SplitFails = cycles.Count(c => c.IsSplitFail);
                PercentSplitfails = SplitFails / cycles.Count() * 100;
                AverageGreenOccupancyPercent = cycles.Average(c => c.GreenOccupancyPercent);
                AverageRedOccupancyPercent = cycles.Average(c => c.RedOccupancyPercent);
            }
        }

        public double AverageRedOccupancyPercent { get; }

        public double AverageGreenOccupancyPercent { get; }

        public double PercentSplitfails { get; }

        public double SplitFails { get; }

        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
    }
}