using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.AggregationRepositories;

namespace ATSPM.Application.Business.LeftTurnGapReport
{

    public class LeftTurnSplitFailAnalysis
    {
        private readonly IApproachRepository _approachRepository;
        private readonly IApproachSplitFailAggregationRepository _approachSplitFailAggregationRepository;

        public LeftTurnSplitFailAnalysis(IApproachRepository approachRepository,
                                         IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository)
        {
            _approachRepository = approachRepository;
            _approachSplitFailAggregationRepository = approachSplitFailAggregationRepository;
        }
        public SplitFailResult GetSplitFailPercent(int approachId,
                                                   DateTime start,
                                                   DateTime end,
                                                   TimeSpan startTime,
                                                   TimeSpan endTime,
                                                   int[] daysOfWeek)
        {

            var approach = _approachRepository.Lookup(approachId);
            var phaseNumber = approach.PermissivePhaseNumber.HasValue ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber;
            List<ApproachSplitFailAggregation> splitFailsAggregates = GetSplitFailAggregates(approachId, phaseNumber, start, end, startTime, endTime, daysOfWeek);
            Dictionary<DateTime, double> percentCyclesWithSplitFail = GetPercentCyclesWithSplitFails(start, end, startTime, endTime, daysOfWeek, splitFailsAggregates);
            int cycles = splitFailsAggregates.Sum(s => s.Cycles);
            int splitFails = splitFailsAggregates.Sum(s => s.SplitFailures);
            if (cycles == 0)
                throw new ArithmeticException("Cycles cannot be zero");
            return new SplitFailResult
            {
                CyclesWithSplitFails = splitFails,
                SplitFailPercent = Convert.ToDouble(splitFails) / Convert.ToDouble(cycles),
                PercentCyclesWithSplitFailList = percentCyclesWithSplitFail,
                Direction = approach.DirectionType.Abbreviation + approach.Detectors.FirstOrDefault()?.MovementType,
            };

        }

        private List<ApproachSplitFailAggregation> GetSplitFailAggregates(int approachId,
                                                                                 int phaseNumber,
                                                                                 DateTime start,
                                                                                 DateTime end,
                                                                                 TimeSpan startTime,
                                                                                 TimeSpan endTime,
                                                                                 int[] daysOfWeek)
        {
            var approach = _approachRepository.Lookup(approachId);
            List<ApproachSplitFailAggregation> splitFailsAggregates = new List<ApproachSplitFailAggregation>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                if (daysOfWeek.Contains((int)start.DayOfWeek))
                {
                    //HACK: had to change this, but it needs location identifier. I put null in here for now
                    splitFailsAggregates.AddRange(_approachSplitFailAggregationRepository
                        .GetAggregationsBetweenDates(null, tempDate.Date.Add(startTime), tempDate.Date.Add(endTime))
                        .GetByApproachId(approach.Id, approach.ProtectedPhaseNumber == phaseNumber));
                }
            }

            return splitFailsAggregates;
        }

        public static Dictionary<DateTime, double> GetPercentCyclesWithSplitFails(DateTime start,
                                                                                   DateTime end,
                                                                                   TimeSpan startTime,
                                                                                   TimeSpan endTime,
                                                                                   int[] daysOfWeek,
                                                                                   List<ApproachSplitFailAggregation> splitFailsAggregates)
        {
            Dictionary<DateTime, double> percentCyclesWithSplitFail = new Dictionary<DateTime, double>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                if (daysOfWeek.Contains((int)tempDate.DayOfWeek))
                {
                    for (var tempstart = tempDate.Date.Add(startTime); tempstart < tempDate.Add(endTime); tempstart = tempstart.AddMinutes(15))
                    {
                        var tempEndTime = tempstart.AddMinutes(15);
                        double tempSplitFails = splitFailsAggregates.Where(s => s.BinStartTime >= tempstart && s.BinStartTime < tempEndTime).Sum(s => s.SplitFailures);
                        var tempCycles = splitFailsAggregates.Where(s => s.BinStartTime >= tempstart && s.BinStartTime < tempEndTime).Sum(s => s.Cycles);
                        double tempPercentFails = 0;
                        if (tempCycles != 0)
                            tempPercentFails = tempSplitFails / tempCycles;
                        percentCyclesWithSplitFail.Add(tempstart, tempPercentFails);
                    }
                }
            }

            return percentCyclesWithSplitFail;
        }
    }
}