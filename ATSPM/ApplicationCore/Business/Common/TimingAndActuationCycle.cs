#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/TimingAndActuationCycle.cs
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

namespace ATSPM.Application.Business.Common
{
    public class TimingAndActuationCycle
    {
        public TimingAndActuationCycle(DateTime startGreen, DateTime endMinGreen, DateTime startYellow, DateTime startRedClearance, DateTime startRed, DateTime endRed, DateTime overlapDark)
        {
            StartGreen = startGreen;
            EndMinGreen = endMinGreen;
            StartYellow = startYellow;
            StartRedClearance = startRedClearance;
            StartRed = startRed;
            EndRed = endRed;
            OverLapDark = overlapDark;
        }
        public DateTime StartGreen { get; set; }
        public DateTime EndMinGreen { get; set; }
        public DateTime StartYellow { get; set; }
        public DateTime StartRedClearance { get; set; }
        public DateTime StartRed { get; set; }
        public DateTime EndRed { get; set; }
        public DateTime OverLapDark { get; set; }
    }
}
