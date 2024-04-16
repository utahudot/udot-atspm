using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.YellowRedActivations
{
    public class YellowRedActivationPlan : Plan
    {
        protected int cycleCount;
        protected DateTime endTime;
        private readonly string planNumber;
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