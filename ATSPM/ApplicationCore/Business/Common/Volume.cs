#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/Volume.cs
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

namespace ATSPM.Application.Business.Common
{
    public class Volume
    {
        private readonly int binSizeMultiplier;

        //private int yAxis;

        public Volume(DateTime startTime, DateTime endTime, int binSize)
        {
            StartTime = startTime;
            EndTime = endTime;
            if (binSize == 0)
                binSizeMultiplier = 0;
            else
                binSizeMultiplier = 60 / binSize;
        }

        public DateTime StartTime { get; }

        public DateTime EndTime { get; }

        public int DetectorCount { get; set; }

        public int HourlyVolume => DetectorCount * binSizeMultiplier;
    }
}