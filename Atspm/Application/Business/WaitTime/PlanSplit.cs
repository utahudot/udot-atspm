﻿#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.WaitTime/PlanSplit.cs
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

namespace Utah.Udot.Atspm.Business.WaitTime
{
    public class PlanSplit
    {
        public PlanSplit(DateTime start, DateTime end, int phaseNumber, int split)
        {
            Start = start;
            End = end;
            PhaseNumber = phaseNumber;
            Split = split;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int PhaseNumber { get; set; }
        public int Split { get; set; }
    }
}