using ATSPM.Application.TempExtensions;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class LeftTurnReportService
    {
        public LeftTurnReportService()
        {
        }

        public Dictionary<TimeSpan, double> GetAMPMPeakGapOut(
            Dictionary<TimeSpan, int> peaks,
            Approach approach,
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            List<PhaseCycleAggregation> cycleAggregations,
            List<PhaseTerminationAggregation> phaseTerminationAggregations)
        {
            int phaseNumber = GetLTPhaseNumberByDirection(approach);
            var maxCycles = GetMaxCycles(approach, phaseNumber, startDate, endDate, peaks, cycleAggregations);
            Dictionary<TimeSpan, double> averageGapOuts = GetAverageGapOutsForPhase(phaseNumber, startDate, endDate, amStartTime, peaks, phaseTerminationAggregations);
            return GetPercentageOfGapOuts(maxCycles, averageGapOuts);
        }

        public static Dictionary<TimeSpan, double> GetPercentageOfGapOuts(Dictionary<TimeSpan, double> maxCycles, Dictionary<TimeSpan, double> averageGapOuts)
        {
            if (averageGapOuts is null)
            {
                throw new ArgumentNullException(nameof(averageGapOuts));
            }

            if (maxCycles is null)
            {
                throw new ArgumentNullException(nameof(maxCycles));
            }

            if (maxCycles.Values.Contains(0))
            {
                throw new ArithmeticException("Max Cycles cannot be zero");
            }

            if (!averageGapOuts.Keys.Contains(maxCycles.Keys.Min()) ||
                !averageGapOuts.Keys.Contains(maxCycles.Keys.Max()))
            {
                throw new IndexOutOfRangeException("Peak hours must be the same for Average Gap Outs and Max Cycles");
            }
            var amPeak = averageGapOuts.Keys.Min();
            var pmPeak = averageGapOuts.Keys.Max();
            Dictionary<TimeSpan, double> percentages = new Dictionary<TimeSpan, double>
            {
                { amPeak, averageGapOuts[amPeak] / maxCycles[amPeak] },
                { pmPeak, averageGapOuts[pmPeak] / maxCycles[pmPeak] }
            };
            return percentages;
        }

        private Dictionary<TimeSpan, double> GetAverageGapOutsForPhase(int phaseNumber, DateTime startDate, DateTime endDate, TimeSpan amStartTime, Dictionary<TimeSpan, int> peaks, List<PhaseTerminationAggregation> phaseTerminationAggregations)
        {
            Dictionary<TimeSpan, double> averages = new Dictionary<TimeSpan, double>();
            var amPeak = peaks.Min(p => p.Key);
            List<double> amGapOutCount = new List<double>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                amGapOutCount.Add(phaseTerminationAggregations.Where(p => p.PhaseNumber == phaseNumber
                                                                            && p.Start >= tempDate.Date.Add(amPeak)
                                                                            && p.Start < tempDate.Date.Add(amPeak).AddHours(1))
                                                                .Sum(g => g.GapOuts));
            }
            LoadGapOutAverages(averages, amPeak, amGapOutCount);

            var pmPeak = peaks.Max(p => p.Key);
            List<double> pmGapOutCount = new List<double>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                pmGapOutCount.Add(phaseTerminationAggregations.Where(p => p.PhaseNumber == phaseNumber
                                                                            && p.Start >= tempDate.Date.Add(pmPeak)
                                                                            && p.Start < tempDate.Date.Add(pmPeak).AddHours(1))
                                                                .Sum(g => g.GapOuts));
            }
            LoadGapOutAverages(averages, pmPeak, pmGapOutCount);

            return averages;
        }

        public static void LoadGapOutAverages(Dictionary<TimeSpan, double> averages, TimeSpan peak, List<double> aggregations)
        {
            if (aggregations.Count > 0)
                averages.Add(peak, aggregations.Average(a => a));
            else
                averages.Add(peak, 0);
        }

        private Dictionary<TimeSpan, double> GetMaxCycles(Approach approach, int phaseNumber, DateTime startDate, DateTime endDate, Dictionary<TimeSpan, int> peaks, List<PhaseCycleAggregation> cycleAggregations)
        {
            var maxCycles = new Dictionary<TimeSpan, double>();
            var amCycleCount = new List<int>();
            var amMaxCycle = 0;
            var pmMaxCycle = 0;
            for (var tempDate = startDate; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                var result = cycleAggregations.Where(r => r.LocationIdentifier == approach.Location.LocationIdentifier
                                                            && r.PhaseNumber == phaseNumber
                                                            && r.Start >= tempDate.Add(peaks.First().Key)
                                                            && r.Start < tempDate.Add(peaks.First().Key).AddHours(1))
                                                .Sum(p => p.TotalGreenToGreenCycles);
                if (result > amMaxCycle)
                    amMaxCycle = result;
            }
            maxCycles.Add(peaks.First().Key, amMaxCycle);
            var pmCycleCount = new List<int>();
            for (var tempDate = startDate; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                var result = cycleAggregations.Where(r => r.LocationIdentifier == approach.Location.LocationIdentifier
                                                            && r.PhaseNumber == phaseNumber
                                                            && r.Start >= tempDate.Add(peaks.Last().Key)
                                                            && r.Start < tempDate.Add(peaks.Last().Key).AddHours(1))
                                                .Sum(p => p.TotalGreenToGreenCycles);
                if (result > pmMaxCycle)
                    pmMaxCycle = result;
            }
            maxCycles.Add(peaks.Last().Key, pmMaxCycle);
            return maxCycles;
        }

        private int GetLTPhaseNumberByDirection(Approach approach)
        {
            return approach.PermissivePhaseNumber ?? approach.ProtectedPhaseNumber;
        }

        public Dictionary<TimeSpan, int> GetAMPMPeakFlowRate(
            Approach approach,
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime,
            int[] daysOfWeek,
            List<DetectorEventCountAggregation> detectorEventCountAggregation)
        {
            List<Detector> detectors = GetLaneByLaneOrAdvanceDetectorsForLocation(approach.Location);
            if (!detectors.Any())
            {
                throw new NotSupportedException("No Detectors found");
            }
            List<DetectorEventCountAggregation> volumeAggregations =
                GetDetectorVolumebyDetector(
                    detectors,
                    startDate,
                    endDate,
                    amStartTime,
                    amEndTime,
                    pmStartTime,
                    pmEndTime,
                    daysOfWeek,
                    detectorEventCountAggregation);
            if (!volumeAggregations.Any())
            {
                throw new NotSupportedException("No Detector Activation Aggregations found");
            }
            List<TimeSpan> distinctTimeSpans = volumeAggregations.Select(v => v.BinStartTime.TimeOfDay).Distinct().OrderBy(v => v).ToList();

            Dictionary<TimeSpan, int> averageByBin = GetAveragesForBinsByTimeSpan(volumeAggregations, distinctTimeSpans);

            Dictionary<TimeSpan, int> hourlyFlowRates = GetHourlyFlowRates(distinctTimeSpans, averageByBin);

            var allDetectorsFlowRate = GetAmPmPeaks(amStartTime, amEndTime, pmStartTime, pmEndTime, hourlyFlowRates);

            return GetLeftTurnAMPMPeakFlowRates(
                approach,
                startDate,
                endDate,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                daysOfWeek,
                detectorEventCountAggregation,
                distinctTimeSpans,
                allDetectorsFlowRate);

        }

        private static Dictionary<TimeSpan, int> GetLeftTurnAMPMPeakFlowRates(Approach approach,
                                                                              DateTime startDate,
                                                                              DateTime endDate,
                                                                              TimeSpan amStartTime,
                                                                              TimeSpan amEndTime,
                                                                              TimeSpan pmStartTime,
                                                                              TimeSpan pmEndTime,
                                                                              int[] daysOfWeek,
                                                                              List<DetectorEventCountAggregation> detectorEventCountAggregation,
                                                                              List<TimeSpan> distinctTimeSpans,
                                                                              Dictionary<TimeSpan, int> allDetectorsFlowRate)
        {
            List<Detector> leftTurndetectors = GetLeftTurnLaneByLaneDetectorsForSignal(approach.Location.GetDetectorsForLocation());
            if (!leftTurndetectors.Any())
            {
                throw new NotSupportedException("No Left Turn Detectors found");
            }
            List<DetectorEventCountAggregation> leftTurnVolumeAggregations =
                GetDetectorVolumebyDetector(leftTurndetectors, startDate, endDate, amStartTime,
                amEndTime, pmStartTime, pmEndTime, daysOfWeek, detectorEventCountAggregation);
            if (!leftTurnVolumeAggregations.Any())
            {
                throw new NotSupportedException("No Left Turn Detector Activation Aggregations found");
            }
            Dictionary<TimeSpan, int> leftTurnAverageByBin = GetAveragesForBinsByApproach(leftTurnVolumeAggregations, distinctTimeSpans, approach.Id);
            Dictionary<TimeSpan, int> leftTurnHourlyFlowRates = GetHourlyFlowRates(distinctTimeSpans, leftTurnAverageByBin);
            Dictionary<TimeSpan, int> leftTurnAmPmPeaks = new Dictionary<TimeSpan, int>
            {
                {
                    allDetectorsFlowRate.First().Key,
                    leftTurnHourlyFlowRates.Where(a => a.Key == allDetectorsFlowRate.First().Key).First().Value
                },
                {
                    allDetectorsFlowRate.Last().Key,
                    leftTurnHourlyFlowRates.Where(a => a.Key == allDetectorsFlowRate.Last().Key).First().Value
                }
            };
            return leftTurnAmPmPeaks;
        }

        public static Dictionary<TimeSpan, int> GetAveragesForBinsByApproach(List<DetectorEventCountAggregation> volumeAggregations,
                                                                  List<TimeSpan> distinctTimeSpans,
                                                                  int approachId)
        {
            Dictionary<TimeSpan, int> averageByBin = new Dictionary<TimeSpan, int>();

            foreach (TimeSpan time in distinctTimeSpans)
            {
                int average = Convert.ToInt32(
                    Math.Round(volumeAggregations
                    .Where(v => v.Start.TimeOfDay == time)
                    .Where(v => v.ApproachId == approachId)
                    .Average(v => v.EventCount)
                    ));
                averageByBin.Add(time, average);
            }

            return averageByBin;
        }

        public static List<Detector> GetLeftTurnLaneByLaneDetectorsForSignal(List<Detector> detectors)
        {
            List<Detector> detectorsList = new List<Detector>();
            foreach (var detector in detectors)
            {
                var detectionTypeIdList = detector.DetectionTypes.Select(d => (int)d.Id).ToList();
                if (detectionTypeIdList.Contains(4) && detector.MovementType == Data.Enums.MovementTypes.L)
                    detectorsList.Add(detector);
            }
            if (detectorsList.Count > 0)
                return detectorsList;

            foreach (var detector in detectors)
            {
                var detectionTypeIdList = detector.DetectionTypes.Select(d => (int)d.Id).ToList();
                if (detectionTypeIdList.Contains(6) && detector.MovementType == Data.Enums.MovementTypes.L)
                    detectorsList.Add(detector);
            }
            return detectorsList;
        }

        public static Dictionary<TimeSpan, int> GetAmPmPeaks(TimeSpan amStartTime,
                                                             TimeSpan amEndTime,
                                                             TimeSpan pmStartTime,
                                                             TimeSpan pmEndTime,
                                                             Dictionary<TimeSpan, int> hourlyFlowRates)
        {
            var amPeak = hourlyFlowRates.Where(h => h.Key >= amStartTime && h.Key < amEndTime)
                            .OrderByDescending(h => h.Value)
                            .First();

            var pmPeak = hourlyFlowRates.Where(h => h.Key >= pmStartTime && h.Key < pmEndTime)
                .OrderByDescending(h => h.Value)
                .First();

            var returnPeaks = new Dictionary<TimeSpan, int>
            {
                { amPeak.Key, amPeak.Value },
                { pmPeak.Key, pmPeak.Value }
            };
            return returnPeaks;
        }

        public static Dictionary<TimeSpan, int> GetHourlyFlowRates(
            List<TimeSpan> distinctTimeSpans,
            Dictionary<TimeSpan, int> averageByBin)
        {
            var hourlyFlowRates = new Dictionary<TimeSpan, int>();
            foreach (var timeSpan in distinctTimeSpans)
            {
                TimeSpan hourEnd = timeSpan.Add(TimeSpan.FromHours(1));
                hourlyFlowRates.Add(timeSpan, averageByBin.Where(d => d.Key >= timeSpan && d.Key < hourEnd).Sum(d => d.Value));
            }

            return hourlyFlowRates;
        }

        public static Dictionary<TimeSpan, int> GetAveragesForBinsByTimeSpan(
            List<DetectorEventCountAggregation> volumeAggregations,
            List<TimeSpan> distinctTimeSpans)
        {
            Dictionary<TimeSpan, int> averageByBin = new Dictionary<TimeSpan, int>();
            foreach (TimeSpan time in distinctTimeSpans)
            {
                var average = Convert.ToInt32(volumeAggregations
                    .Where(v => v.Start.TimeOfDay == time)
                    .GroupBy(v => v.Start.Day).Select(v => new
                    {
                        sum = v.Sum(a => a.EventCount)
                    }).Average(v => v.sum)
                );

                averageByBin.Add(time, average);
            };
            return averageByBin;
        }

        public static List<DetectorEventCountAggregation> GetDetectorVolumebyDetector(List<Detector> detectors, DateTime startDate,
            DateTime endDate, TimeSpan amStartTime, TimeSpan amEndTime, TimeSpan pmStartTime, TimeSpan pmEndTime,
            int[] daysOfWeek, List<DetectorEventCountAggregation> detectorEventCountAggregations)
        {
            var detectorAggregations = new List<DetectorEventCountAggregation>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                if (daysOfWeek.Contains((int)startDate.DayOfWeek))
                    foreach (var detector in detectors)
                    {
                        detectorAggregations.AddRange(detectorEventCountAggregations.Where(d => d.DetectorPrimaryId == detector.Id && d.Start >= tempDate.Add(amStartTime) && d.Start <= tempDate.Add(amEndTime)));
                        detectorAggregations.AddRange(detectorEventCountAggregations.Where(d => d.DetectorPrimaryId == detector.Id && d.Start >= tempDate.Add(pmStartTime) && d.Start <= tempDate.Add(pmEndTime)));
                    }
            }
            return detectorAggregations;
        }


        public static List<Detector> GetLaneByLaneOrAdvanceDetectorsForLocation(Location location)
        {
            var detectors = location.GetDetectorsForLocation();
            var laneByLaneDetectors = detectors.Where(d => d.DetectionTypes.Select(dt => (int)dt.Id).Contains(4)).ToList();
            if (laneByLaneDetectors.Count > 0)
                return laneByLaneDetectors;
            var advanceDetectors = detectors.Where(d => d.DetectionTypes.Select(dt => (int)dt.Id).Contains(6)).ToList();
            if (advanceDetectors.Count > 0)
                return advanceDetectors;
            return new List<Detector>();
        }


        public Dictionary<TimeSpan, double> GetAMPMPeakPedCyclesPercentages(
            Dictionary<TimeSpan, int> peaks,
            Approach approach,
            int opposingPhase,
            DateTime startDate,
            DateTime endDate,
            List<PhaseCycleAggregation> cycleAggregations,
            List<PhasePedAggregation> pedAggregations
            )
        {
            Dictionary<TimeSpan, double> averageCyles = GetAverageAggregationsForPeaks(approach.Location.LocationIdentifier, opposingPhase, startDate, endDate, peaks, cycleAggregations, item => item.TotalRedToRedCycles);
            Dictionary<TimeSpan, double> averagePedCycles = GetAverageAggregationsForPeaks(approach.Location.LocationIdentifier, opposingPhase, startDate, endDate, peaks, pedAggregations, item => item.PedCycles);
            return GetPercentageOfPedCycles(averageCyles, averagePedCycles);
        }

        public static Dictionary<TimeSpan, double> GetPercentageOfPedCycles(
            Dictionary<TimeSpan, double> averageCyles,
            Dictionary<TimeSpan, double> averagePedCycles)
        {

            if (averagePedCycles is null)
            {
                throw new ArgumentNullException(nameof(averagePedCycles));
            }

            if (averageCyles is null)
            {
                throw new ArgumentNullException(nameof(averageCyles));
            }

            if (averageCyles.Values.Contains(0))
            {
                throw new ArithmeticException("Average Gap Out cannot be zero");
            }

            if (!averageCyles.Keys.Contains(averagePedCycles.Keys.Min()) ||
                !averageCyles.Keys.Contains(averagePedCycles.Keys.Max()))
            {
                throw new IndexOutOfRangeException("Peak hours must be the same for Cycles and Ped Cycles");
            }
            var amPeak = averagePedCycles.Keys.Min();
            var pmPeak = averagePedCycles.Keys.Max();
            Dictionary<TimeSpan, double> percentages = new Dictionary<TimeSpan, double>
            {
                { amPeak, averagePedCycles[amPeak] / averageCyles[amPeak] },
                { pmPeak, averagePedCycles[pmPeak] / averageCyles[pmPeak] }
            };
            return percentages;
        }

        private Dictionary<TimeSpan, double> GetAverageAggregationsForPeaks<T>(
             string locationIdentifier,
             int phase,
             DateTime startDate,
             DateTime endDate,
             Dictionary<TimeSpan, int> peaks,
             List<T> aggregations,
             Func<T, double> valueSelector) where T : AggregationModelBase, ILocationPhaseLayer // This selector function allows dynamic column selection
        {
            var result = new Dictionary<TimeSpan, double>();

            foreach (var peak in peaks.Keys)
            {
                var dailyAggregations = new List<double>();
                for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
                {
                    var daySum = aggregations
                        .Where(a => a.LocationIdentifier == locationIdentifier
                                    && a.PhaseNumber == phase
                                    && a.Start >= tempDate.Add(peak)
                                    && a.Start < tempDate.Add(peak).AddHours(1))
                        .Sum(a => valueSelector(a)); // Dynamically select the value based on the passed function

                    dailyAggregations.Add(daySum);
                }

                var average = dailyAggregations.Count > 0 ? dailyAggregations.Average() : 0;
                result.Add(peak, average);
            }

            return result;
        }

        public int GetOpposingPhase(Approach approach)
        {
            //If permissive only 2 = 6, 4 = 8, 6 = 2 and 8 = 4
            if (approach.ProtectedPhaseNumber == 0 && approach.PermissivePhaseNumber.HasValue)
            {
                switch (approach.PermissivePhaseNumber)
                {
                    case 2: return 6;
                    case 4: return 8;
                    case 6: return 2;
                    case 8: return 4;
                    default: throw new ArgumentException("Invalid Phase");
                }
            }
            //1=2, 3=4, 5=6, and 7=8 if there is a protected.
            else
            {
                switch (approach.ProtectedPhaseNumber)
                {
                    case 1: return 2;
                    case 3: return 4;
                    case 5: return 6;
                    case 7: return 8;
                    case 2: return 6;
                    case 4: return 8;
                    case 6: return 2;
                    case 8: return 4;
                    default: throw new ArgumentException("Invalid Phase");
                }
            }

        }


    }

}
