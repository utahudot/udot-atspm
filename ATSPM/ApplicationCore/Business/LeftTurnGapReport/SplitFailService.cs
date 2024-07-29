using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models.AggregationModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.LeftTurnGapReport
{

    public class SplitFailService
    {
        private readonly ILocationRepository locationRepository;

        public SplitFailService(ILocationRepository locationRepository)
        {
            this.locationRepository = locationRepository;
        }
        public LeftTurnSplitFailResult GetSplitFailPercent(LeftTurnSplitFailOptions options, List<ApproachSplitFailAggregation> splitFailsAggregates)
        {
            var location = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start);
            if (location == null)
                throw new ArgumentNullException("Location not found");
            var approach = location.Approaches.Where(a => a.Id == options.ApproachId)?.FirstOrDefault();
            if (approach == null)
                throw new ArgumentNullException("Approach not found");
            var phaseNumber = approach.PermissivePhaseNumber.HasValue ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber;
            Dictionary<DateTime, double> percentCyclesWithSplitFail = GetPercentCyclesWithSplitFails(options, splitFailsAggregates);
            int cycles = splitFailsAggregates.Sum(s => s.Cycles);
            int splitFails = splitFailsAggregates.Sum(s => s.SplitFailures);
            if (cycles == 0)
                throw new ArithmeticException("Cycles cannot be zero");
            return new LeftTurnSplitFailResult
            {
                CyclesWithSplitFails = splitFails,
                SplitFailPercent = (Convert.ToDouble(splitFails) / Convert.ToDouble(cycles)) * 100,
                PercentCyclesWithSplitFailList = percentCyclesWithSplitFail,
                Direction = approach.DirectionType.Abbreviation + approach.Detectors.FirstOrDefault()?.MovementType,
            };

        }



        public static Dictionary<DateTime, double> GetPercentCyclesWithSplitFails(LeftTurnSplitFailOptions options,
                                                                                   List<ApproachSplitFailAggregation> splitFailsAggregates)
        {
            var startTime = new TimeSpan(options.StartHour, options.StartMinute, 0);
            var endTime = new TimeSpan(options.EndHour, options.EndMinute, 0);
            Dictionary<DateTime, double> percentCyclesWithSplitFail = new Dictionary<DateTime, double>();
            for (var tempDate = options.Start.Date; tempDate <= options.End; tempDate = tempDate.AddDays(1))
            {
                if (options.DaysOfWeek.Contains((int)tempDate.DayOfWeek))
                {
                    for (var tempstart = tempDate.Date.Add(startTime); tempstart < tempDate.Add(endTime); tempstart = tempstart.AddMinutes(15))
                    {
                        var tempEndTime = tempstart.AddMinutes(15);
                        var splitFails = splitFailsAggregates.Where(s => s.Start >= tempstart && s.Start < tempEndTime).ToList();
                        double tempSplitFails = splitFails.Sum(s => s.SplitFailures);
                        var tempCycles = splitFails.Sum(s => s.Cycles);
                        double tempPercentFails = 0;
                        if (tempCycles != 0)
                            tempPercentFails = (tempSplitFails / tempCycles) * 100;
                        percentCyclesWithSplitFail.Add(tempstart, tempPercentFails);
                    }
                }
            }

            return percentCyclesWithSplitFail;
        }
    }
}