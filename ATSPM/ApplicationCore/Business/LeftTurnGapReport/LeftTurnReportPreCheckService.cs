using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class LeftTurnReportPreCheckService
    {

        public LeftTurnReportPreCheckService()
        {
        }

        public Dictionary<TimeSpan, double> GetAMPMPeakPedCyclesPercentages(
            int approachId,
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime,
            int[] daysOfWeek,
            Approach approach,
            List<DetectorEventCountAggregation> leftTurnDetectorEventCountAggregations,
            List<DetectorEventCountAggregation> volumeCountAggregations,
            List<PhaseCycleAggregation> phaseCycleAggregations,
            List<PhasePedAggregation> phasePedAggregations)
        {
            Dictionary<TimeSpan, int> peaks = GetAMPMPeakFlowRate(
                startDate,
                endDate,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                daysOfWeek,
                approach,
                leftTurnDetectorEventCountAggregations,
                volumeCountAggregations);
            int opposingPhase = GetOpposingPhase(approach);
            Dictionary<TimeSpan, double> averageCyles = GetAverageCycles(
                approach.Location.LocationIdentifier,
                opposingPhase,
                startDate,
                endDate,
                peaks,
                phaseCycleAggregations);
            Dictionary<TimeSpan, double> averagePedCycles = GetAveragePedCycles(
                approach.Location.LocationIdentifier,
                opposingPhase,
                startDate,
                endDate,
                peaks,
                phasePedAggregations);
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
            Dictionary<TimeSpan, double> percentages = new Dictionary<TimeSpan, double>();
            percentages.Add(amPeak, averagePedCycles[amPeak] / averageCyles[amPeak]);
            percentages.Add(pmPeak, averagePedCycles[pmPeak] / averageCyles[pmPeak]);
            return percentages;
        }

        private Dictionary<TimeSpan, double> GetAveragePedCycles(
            string locationId,
            int phase,
            DateTime startDate,
            DateTime endDate,
            Dictionary<TimeSpan, int> peaks,
            List<PhasePedAggregation> phasePedAggregations)
        {
            Dictionary<TimeSpan, double> averagePedCycles = new Dictionary<TimeSpan, double>();
            List<double> amAggregations = new List<double>();
            var amPeak = peaks.Min(p => p.Key);
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                amAggregations.Add(phasePedAggregations.Where(p =>
                p.locationId == locationId
                && p.PhaseNumber == phase
                && p.BinStartTime >= tempDate.Date.Add(amPeak)
                && p.BinStartTime < tempDate.Date.Add(amPeak).AddHours(1)).Sum(a => a.PedCycles));
                //amAggregations.Add(_phasePedAggregationRepository.GetPhasePedsAggregationBylocationIdPhaseNumberAndDateRange(locationId, phase, tempDate.Date.Add(amPeak), tempDate.Date.Add(amPeak).AddHours(1)).Sum(a => a.PedCycles));
            }
            if (amAggregations.Count > 0)
                averagePedCycles.Add(amPeak, amAggregations.Average(a => a));
            else
                averagePedCycles.Add(amPeak, 0);
            var pmPeak = peaks.Max(p => p.Key);
            List<double> pmAggregations = new List<double>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {

                amAggregations.Add(phasePedAggregations.Where(p =>
                p.locationId == locationId
                && p.PhaseNumber == phase
                && p.BinStartTime >= tempDate.Date.Add(pmPeak)
                && p.BinStartTime < tempDate.Date.Add(pmPeak).AddHours(1)).Sum(a => a.PedCycles));
                //pmAggregations.Add(_phasePedAggregationRepository.GetPhasePedsAggregationBylocationIdPhaseNumberAndDateRange(locationId, phase, tempDate.Date.Add(pmPeak), tempDate.Date.Add(pmPeak).AddHours(1)).Sum(a => a.PedCycles));
            }
            if (pmAggregations.Count > 0)
                averagePedCycles.Add(pmPeak, pmAggregations.Average(a => a));
            else
                averagePedCycles.Add(pmPeak, 0);
            return averagePedCycles;
        }

        private Dictionary<TimeSpan, double> GetAverageCycles(
            string locationId,
            int phase,
            DateTime startDate,
            DateTime endDate,
            Dictionary<TimeSpan, int> peaks,
            List<PhaseCycleAggregation> approachCycleAggregations
            )
        {
            Dictionary<TimeSpan, double> averageCycles = new Dictionary<TimeSpan, double>();
            List<PhaseCycleAggregation> amAggregations = new List<PhaseCycleAggregation>();
            var amPeak = peaks.Min(p => p.Key);
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                amAggregations.AddRange(approachCycleAggregations.Where(a => a.LocationIdentifier == locationId && a.PhaseNumber == phase && a.BinStartTime >= tempDate.Date.Add(amPeak) && a.BinStartTime < tempDate.Date.Add(amPeak).AddHours(1)));
                //amAggregations.AddRange(_approachCycleAggregationRepository.GetApproachCyclesAggregationBylocationIdPhaseAndDateRange(locationId, phase, tempDate.Date.Add(amPeak), tempDate.Date.Add(amPeak).AddHours(1)));
            }
            averageCycles.Add(amPeak, amAggregations.Average(a => a.TotalRedToRedCycles));
            var pmPeak = peaks.Max(p => p.Key);
            List<PhaseCycleAggregation> pmAggregations = new List<PhaseCycleAggregation>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                amAggregations.AddRange(approachCycleAggregations.Where(a => a.LocationIdentifier == locationId && a.PhaseNumber == phase && a.BinStartTime >= tempDate.Date.Add(pmPeak) && a.BinStartTime < tempDate.Date.Add(pmPeak).AddHours(1)));
                //pmAggregations.AddRange(_approachCycleAggregationRepository.GetApproachCyclesAggregationBylocationIdPhaseAndDateRange(locationId, phase, tempDate.Date.Add(pmPeak), tempDate.Date.Add(pmPeak).AddHours(1)));
            }
            averageCycles.Add(pmPeak, pmAggregations.Average(a => a.TotalRedToRedCycles));
            return averageCycles;
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

        public Dictionary<TimeSpan, double> GetAMPMPeakGapOut(
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime,
            int[] daysOfWeek,
            Approach approach,
            List<PhaseTerminationAggregation> phaseTerminationAggregations,
            List<DetectorEventCountAggregation> leftTurnDetectorEventCountAggregations,
            List<DetectorEventCountAggregation> volumeCountAggregations,
            List<PhaseCycleAggregation> cycleAggregations)
        {
            Dictionary<TimeSpan, int> peaks = GetAMPMPeakFlowRate(
                startDate,
                endDate,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                daysOfWeek,
                approach,
                leftTurnDetectorEventCountAggregations,
                volumeCountAggregations);
            int phaseNumber = approach.PermissivePhaseNumber ?? approach.ProtectedPhaseNumber;
            var maxCycles = GetMaxCycles(
                approach.Location.LocationIdentifier,
                phaseNumber,
                startDate,
                endDate,
                peaks,
                cycleAggregations);
            Dictionary<TimeSpan, double> averageGapOuts = GetAverageGapOutsForPhase(
                approach.Location.LocationIdentifier,
                phaseNumber,
                startDate,
                endDate,
                amStartTime,
                peaks,
                phaseTerminationAggregations);
            return GetPercentageOfGapOuts(maxCycles, averageGapOuts);

        }

        private Dictionary<TimeSpan, double> GetMaxCycles(
            string locationId,
            int phaseNumber,
            DateTime startDate,
            DateTime endDate,
            Dictionary<TimeSpan, int> peaks,
            List<PhaseCycleAggregation> cycleAggregations)
        {
            var maxCycles = new Dictionary<TimeSpan, double>();
            var amCycleCount = new List<int>();
            var amMaxCycle = 0;
            var pmMaxCycle = 0;
            for (var tempDate = startDate; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                var result = cycleAggregations.Where(c =>
                c.LocationIdentifier == locationId
                && c.PhaseNumber == phaseNumber
                && c.BinStartTime >= tempDate.Add(peaks.First().Key)
                && c.BinStartTime < tempDate.Add(peaks.First().Key).AddHours(1)).Sum(p => p.TotalGreenToGreenCycles);
                //var result = _approachCycleAggregationRepository.GetCycleCountBylocationIdAndDateRange(locationId, phaseNumber, tempDate.Add(peaks.First().Key), tempDate.Add(peaks.First().Key).AddHours(1));
                if (result > amMaxCycle)
                    amMaxCycle = result;
            }
            maxCycles.Add(peaks.First().Key, amMaxCycle);
            var pmCycleCount = new List<int>();
            for (var tempDate = startDate; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                var result = cycleAggregations.Where(c =>
                c.LocationIdentifier == locationId
                && c.PhaseNumber == phaseNumber
                && c.BinStartTime >= tempDate.Add(peaks.Last().Key)
                && c.BinStartTime < tempDate.Add(peaks.Last().Key).AddHours(1)).Sum(p => p.TotalGreenToGreenCycles);
                //var result = _approachCycleAggregationRepository.GetCycleCountBylocationIdAndDateRange(locationId, phaseNumber, tempDate.Add(peaks.Last().Key), tempDate.Add(peaks.Last().Key).AddHours(1));
                if (result > pmMaxCycle)
                    pmMaxCycle = result;
            }
            maxCycles.Add(peaks.Last().Key, pmMaxCycle);
            return maxCycles;
        }

        public static Dictionary<TimeSpan, double> GetPercentageOfGapOuts(
            Dictionary<TimeSpan, double> maxCycles,
            Dictionary<TimeSpan, double> averageGapOuts)
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
            Dictionary<TimeSpan, double> percentages = new Dictionary<TimeSpan, double>();
            percentages.Add(amPeak, averageGapOuts[amPeak] / maxCycles[amPeak]);
            percentages.Add(pmPeak, averageGapOuts[pmPeak] / maxCycles[pmPeak]);

            //TODO: Change from average terminations to max cycles sum cycles for all phases separately for an hour take the max volume
            return percentages;
        }

        private Dictionary<TimeSpan, double> GetAverageGapOutsForPhase(
            string locationId,
            int phaseNumber,
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            Dictionary<TimeSpan, int> peaks,
            List<PhaseTerminationAggregation> phaseTerminationAggregations)
        {
            Dictionary<TimeSpan, double> averages = new Dictionary<TimeSpan, double>();
            var amPeak = peaks.Min(p => p.Key);
            List<double> amGapOutCount = new List<double>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                amGapOutCount.Add(phaseTerminationAggregations.Where(p =>
                    p.BinStartTime >= tempDate.Date.Add(amPeak)
                    && p.BinStartTime < tempDate.Date.Add(amPeak).AddHours(1))
                    .Sum(g => g.GapOuts));
                //amGapOutCount.Add(_phaseTerminationAggregationRepository.GetPhaseTerminationsAggregationBylocationIdPhaseNumberAndDateRange(locationId, phaseNumber, tempDate.Date.Add(amPeak), tempDate.Date.Add(amPeak).AddHours(1)).Sum(g => g.GapOuts));
            }
            LoadGapOutAverages(averages, amPeak, amGapOutCount);

            var pmPeak = peaks.Max(p => p.Key);
            List<double> pmGapOutCount = new List<double>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                pmGapOutCount.Add(phaseTerminationAggregations.Where(p =>
                    p.BinStartTime >= tempDate.Date.Add(pmPeak)
                    && p.BinStartTime < tempDate.Date.Add(pmPeak).AddHours(1))
                    .Sum(g => g.GapOuts));
                //pmGapOutCount.Add(_phaseTerminationAggregationRepository.GetPhaseTerminationsAggregationBylocationIdPhaseNumberAndDateRange(locationId, phaseNumber, tempDate.Date.Add(pmPeak), tempDate.Date.Add(pmPeak).AddHours(1)).Sum(g => g.GapOuts));
            }
            LoadGapOutAverages(averages, pmPeak, pmGapOutCount);

            return averages;
        }

        public static void LoadGapOutAverages(
            Dictionary<TimeSpan, double> averages,
            TimeSpan peak,
            List<double> aggregations)
        {
            if (aggregations.Count > 0)
                averages.Add(peak, aggregations.Average(a => a));
            else
                averages.Add(peak, 0);
        }

        public Dictionary<TimeSpan, int> GetAMPMPeakFlowRate(
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime,
            int[] daysOfWeek,
            Approach approach,
            List<DetectorEventCountAggregation> leftTurnDetectorEventCountAggregations,
            List<DetectorEventCountAggregation> volumeAggregations)
        {
            List<TimeSpan> distinctTimeSpans = volumeAggregations.Select(v => v.BinStartTime.TimeOfDay).Distinct().OrderBy(v => v).ToList();

            Dictionary<TimeSpan, int> averageByBin = GetAveragesForBinsByTimeSpan(volumeAggregations, distinctTimeSpans);

            Dictionary<TimeSpan, int> hourlyFlowRates = GetHourlyFlowRates(distinctTimeSpans, averageByBin);

            var allDetectorsFlowRate = GetAmPmPeaks(
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                hourlyFlowRates);

            return GetLeftTurnAMPMPeakFlowRates(
                startDate,
                endDate,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                daysOfWeek,
                distinctTimeSpans,
                allDetectorsFlowRate,
                approach,
                leftTurnDetectorEventCountAggregations);
        }

        private Dictionary<TimeSpan, int> GetLeftTurnAMPMPeakFlowRates(
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime,
            int[] daysOfWeek,
            List<TimeSpan> distinctTimeSpans,
            Dictionary<TimeSpan, int> allDetectorsFlowRate,
            Approach approach,
            List<DetectorEventCountAggregation> leftTurnVolumeAggregations)
        {
            if (!leftTurnVolumeAggregations.Any())
            {
                throw new NotSupportedException("No Left Turn Detector Activation Aggregations found");
            }
            Dictionary<TimeSpan, int> leftTurnAverageByBin = GetAveragesForBinsByApproach(leftTurnVolumeAggregations, distinctTimeSpans, approach.Id); //add approach id
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

        public Dictionary<TimeSpan, int> GetAmPmPeaks(
            TimeSpan amStartTime,
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

            var returnPeaks = new Dictionary<TimeSpan, int>();
            returnPeaks.Add(amPeak.Key, amPeak.Value);
            returnPeaks.Add(pmPeak.Key, pmPeak.Value);
            return returnPeaks;
        }

        public Dictionary<TimeSpan, int> GetHourlyFlowRates(
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

        public Dictionary<TimeSpan, int> GetAveragesForBinsByTimeSpan(
            List<DetectorEventCountAggregation> volumeAggregations,
            List<TimeSpan> distinctTimeSpans)
        {
            Dictionary<TimeSpan, int> averageByBin = new Dictionary<TimeSpan, int>();
            foreach (TimeSpan time in distinctTimeSpans)
            {
                var average = Convert.ToInt32(volumeAggregations
                    .Where(v => v.BinStartTime.TimeOfDay == time)
                    .GroupBy(v => v.BinStartTime.Day).Select(v => new
                    {
                        sum = v.Sum(a => a.EventCount)
                    }).Average(v => v.sum)
                );

                averageByBin.Add(time, average);
            };
            return averageByBin;
        }

        public Dictionary<TimeSpan, int> GetAveragesForBinsByApproach(
            List<DetectorEventCountAggregation> volumeAggregations,
            List<TimeSpan> distinctTimeSpans,
            int approachId)
        {
            Dictionary<TimeSpan, int> averageByBin = new Dictionary<TimeSpan, int>();

            foreach (TimeSpan time in distinctTimeSpans)
            {
                int average = Convert.ToInt32(
                    Math.Round(volumeAggregations
                    .Where(v => v.BinStartTime.TimeOfDay == time)
                    .Where(v => v.ApproachId == approachId)
                    .Average(v => v.EventCount)
                    ));
                averageByBin.Add(time, average);
            }

            return averageByBin;
        }

        public List<Detector> GetLeftTurnDetectors(
            Approach approach)
        {
            var movementTypes = new List<MovementTypes>() { MovementTypes.L };
            //only return detector types of type 4
            return approach.Detectors.Where(d =>
            d.DetectionTypes.Select(t => t.Id).Contains(DetectionTypes.LLC)
            && movementTypes.Contains(d.MovementType)).ToList();
        }




    }
}
