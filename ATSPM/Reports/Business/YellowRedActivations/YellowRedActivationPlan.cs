﻿using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.Application.Reports.Business.YellowRedActivations
{
    public class YellowRedActivationPlan
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
            double srlvSeconds,
            Approach approach)
        {
            Approach = approach;
            startTime = start;
            endTime = end;
            this.planNumber = planNumber;
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
            string plan,
            double srlvSeconds,
            Approach approach)
        {
            SRLVSeconds = srlvSeconds;
            startTime = start;
            endTime = end;
            planNumber = plan;
            Approach = approach;
        }

        public DateTime StartTime => startTime;

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

        public DateTime EndTime => endTime;

        public int CycleCount
        {
            get => cycleCount;
            set => cycleCount = value;
        }

        public int CycleLength { get; set; }

        public int OffsetLength { get; set; }

        public string PlanNumber => planNumber;

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

        public double AverageTRLV => Math.Round(TotalViolationTime / Violations, 1);

        public double AverageTYLO => Math.Round(TotalYellowTime / YellowOccurrences, 1);

        public double PercentViolations
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(Violations / TotalVolume * 100, 0);
                return 0;
            }
        }

        public double PercentYellowOccurrences
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(YellowOccurrences / TotalVolume * 100, 0);
                return 0;
            }
        }

        public double PercentSevereViolations
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(SevereRedLightViolations / TotalVolume * 100, 2);
                return 0;
            }
        }

        public Approach Approach { get; set; }
        public double ViolationTime
        {
            get { return rlmCycleCollection.Sum(c => c.TotalViolationTime); }
        }

        

        

       
    }
}