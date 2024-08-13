﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.YellowRedActivations/YellowRedActivationPlan.cs
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

namespace Utah.Udot.Atspm.Business.YellowRedActivations
{
    public class YellowRedActivationPlan : Plan
    {
        protected int cycleCount;
        protected DateTime endTime;
        protected List<YellowRedActivationsCycle> rlmCycleCollection = new List<YellowRedActivationsCycle>();
        public SortedDictionary<int, int> Splits = new SortedDictionary<int, int>();
        protected DateTime startTime;


        public YellowRedActivationPlan(
            DateTime start,
            DateTime end,
            string planNumber,
            List<YellowRedActivationsCycle> cycles,
            double srlvSeconds) : base(planNumber, start, end
            )
        {
            //startTime = start;
            //endTime = end;
            //this.planNumber = planNumber;
            SRLVSeconds = srlvSeconds;
            startTime = start;
            endTime = end;
            RLMCycleCollection = cycles.Where(c => c.StartTime >= startTime && c.StartTime < endTime).ToList();
            //GetRedCycle(start, end, cycleEvents);
            CycleCount = RLMCycleCollection.Count;
            TotalVolume = cycles == null ? 0 : cycles.SelectMany(c => c.DetectorActivations).Count();
        }

        public YellowRedActivationPlan(
            DateTime start,
            DateTime end,
            string planNumber,
            double srlvSeconds) : base(planNumber, start, end
            )
        {
            SRLVSeconds = srlvSeconds;
            //startTime = start;
            //endTime = end;
            //planNumber = plan;
        }

        //public DateTime StartTime => startTime;

        public double Violations
        {
            get { return RLMCycleCollection.Sum(d => d.Violations); }
        }

        public double YellowOccurrences
        {
            get { return RLMCycleCollection.Sum(d => d.YellowOccurrences); }
        }

        public List<YellowRedActivationsCycle> RLMCycleCollection
        {
            get => rlmCycleCollection;
            set => rlmCycleCollection = value;
        }

        //public DateTime EndTime => endTime;

        public int CycleCount
        {
            get => cycleCount;
            set => cycleCount = value;
        }

        public int CycleLength { get; set; }

        public int OffsetLength { get; set; }

        //public string PlanNumber => planNumber;

        public double SRLVSeconds { get; }

        public double SevereRedLightViolations
        {
            get { return RLMCycleCollection.Sum(d => d.SevereRedLightViolations); }
        }

        public double TotalVolume { get; private set; }


        public double TotalViolationTime
        {
            get { return RLMCycleCollection.Sum(d => d.TotalViolationTime); }
        }

        public double TotalYellowTime
        {
            get { return RLMCycleCollection.Sum(d => d.TotalYellowTime); }
        }

        public double AverageTRLV
        {
            get
            {
                if (Violations <= 0)
                {
                    return 0;
                }
                return TotalViolationTime / Violations;
            }
        }

        public double AverageTYLO => TotalYellowTime / YellowOccurrences;

        public double PercentViolations
        {
            get
            {
                if (TotalVolume > 0)
                    return Violations / TotalVolume * 100;
                return 0;
            }
        }

        public double PercentYellowOccurrences
        {
            get
            {
                if (TotalVolume > 0)
                    return YellowOccurrences / TotalVolume * 100;
                return 0;
            }
        }

        public double PercentSevereViolations
        {
            get
            {
                if (TotalVolume > 0)
                    return SevereRedLightViolations / TotalVolume * 100;
                return 0;
            }
        }

        public double ViolationTime
        {
            get { return rlmCycleCollection.Sum(c => c.TotalViolationTime); }
        }






    }
}