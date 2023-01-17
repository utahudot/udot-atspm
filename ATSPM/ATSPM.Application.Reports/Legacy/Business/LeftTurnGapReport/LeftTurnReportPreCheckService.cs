using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Data.Enums;

namespace Legacy.Common.Business.LeftTurnGapReport
{
    public class LeftTurnReportPreCheckService
    {
        private readonly ISignalRepository signalRepository;
        private readonly IApproachRepository approachRepository;
        private readonly IDetectorRepository detectorRepository;
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;
        private readonly IPhaseCycleAggregationRepository phaseCycleAggregationRepository;
        private readonly IPhasePedAggregationRepository phasePedAggregationRepository;
        private readonly IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository;

        public LeftTurnReportPreCheckService(
            ISignalRepository signalRepository,
            IApproachRepository approachRepository,
            IDetectorRepository detectorRepository,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository)
        {
            this.signalRepository = signalRepository;
            this.approachRepository = approachRepository;
            this.detectorRepository = detectorRepository;
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
            this.phasePedAggregationRepository = phasePedAggregationRepository;
            this.phaseTerminationAggregationRepository = phaseTerminationAggregationRepository;
        }

        public Dictionary<TimeSpan, double> GetAMPMPeakPedCyclesPercentages(
            string signalId,
            DirectionTypes directionType,
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime)
        {
            Dictionary<TimeSpan, int> peaks = GetAMPMPeakFlowRate(
                signalId,
                directionType,
                startDate,
                endDate,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime);
            Approach approach = GetLTPhaseNumberPhaseTypeByDirection(signalId, directionType);
            int opposingPhase = GetOpposingPhase(approach);
            Dictionary<TimeSpan, double> averageCyles = GetAverageCycles(
                signalId,
                opposingPhase,
                startDate,
                endDate,
                peaks);
            Dictionary<TimeSpan, double> averagePedCycles = GetAveragePedCycles(
                signalId,
                opposingPhase,
                startDate,
                endDate,
                peaks);
            return GetPercentageOfPedCycles(averageCyles, averagePedCycles);
        }

        private Dictionary<TimeSpan, double> GetPercentageOfPedCycles(Dictionary<TimeSpan, double> averageCyles, Dictionary<TimeSpan, double> averagePedCycles)
        {
            if (averageCyles.Values.Contains(0))
            {
                throw new ArithmeticException("Average Gap Out cannot be zero");
            }
            if (!averageCyles.Keys.Contains(averagePedCycles.Keys.Min()) ||
                !averageCyles.Keys.Contains(averagePedCycles.Keys.Max()))
            {
                throw new IndexOutOfRangeException("Peak hours do not align for AM/PM peak");
            }
            var amPeak = averagePedCycles.Keys.Min();
            var pmPeak = averagePedCycles.Keys.Max();
            Dictionary<TimeSpan, double> percentages = new Dictionary<TimeSpan, double>();
            percentages.Add(amPeak, averagePedCycles[amPeak] / averageCyles[amPeak]);
            percentages.Add(pmPeak, averagePedCycles[pmPeak] / averageCyles[pmPeak]);
            return percentages;
        }

        private Dictionary<TimeSpan, double> GetAveragePedCycles(
            string signalId,
            int phase,
            DateTime startDate,
            DateTime endDate,
            Dictionary<TimeSpan, int> peaks)
        {
            Dictionary<TimeSpan, double> averagePedCycles = new Dictionary<TimeSpan, double>();
            List<PhasePedAggregation> amAggregations = new List<PhasePedAggregation>();
            var amPeak = peaks.Min(p => p.Key);
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                amAggregations.AddRange(phasePedAggregationRepository.GetPhasePedsAggregationBySignalIdPhaseNumberAndDateRange(signalId, phase, tempDate.Date.Add(amPeak), tempDate.Date.Add(amPeak).AddHours(1)));
            }
            averagePedCycles.Add(amPeak, amAggregations.Average(a => a.PedCycles));
            var pmPeak = peaks.Max(p => p.Key);
            List<PhasePedAggregation> pmAggregations = new List<PhasePedAggregation>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                pmAggregations.AddRange(phasePedAggregationRepository.GetPhasePedsAggregationBySignalIdPhaseNumberAndDateRange(signalId, phase, tempDate.Date.Add(pmPeak), tempDate.Date.Add(pmPeak).AddHours(1)));
            }
            averagePedCycles.Add(amPeak, pmAggregations.Average(a => a.PedCycles));
            return averagePedCycles;
        }

        private Dictionary<TimeSpan, double> GetAverageCycles(
            string signalId,
            int phase,
            DateTime startDate,
            DateTime endDate,
            Dictionary<TimeSpan, int> peaks)
        {
            Dictionary<TimeSpan, double> averageCycles = new Dictionary<TimeSpan, double>();
            List<PhaseCycleAggregation> amAggregations = new List<PhaseCycleAggregation>();
            var amPeak = peaks.Min(p => p.Key);
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                amAggregations.AddRange(phaseCycleAggregationRepository.GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(signalId, phase, tempDate.Date.Add(amPeak), tempDate.Date.Add(amPeak).AddHours(1)));
            }
            averageCycles.Add(amPeak, amAggregations.Average(a => a.TotalRedToRedCycles));
            var pmPeak = peaks.Max(p => p.Key);
            List<PhaseCycleAggregation> pmAggregations = new List<PhaseCycleAggregation>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                pmAggregations.AddRange(phaseCycleAggregationRepository.GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(signalId, phase, tempDate.Date.Add(pmPeak), tempDate.Date.Add(pmPeak).AddHours(1)));
            }
            averageCycles.Add(amPeak, pmAggregations.Average(a => a.TotalRedToRedCycles));
            return averageCycles;
        }

        public int GetOpposingPhase(Approach approach)
        {
            int opposingPhase = 0;
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
                    case 2: return 1;
                    case 3: return 4;
                    case 4: return 3;
                    case 5: return 6;
                    case 6: return 5;
                    case 7: return 8;
                    case 8: return 7;
                    default: throw new ArgumentException("Invalid Phase");
                }
            }

        }

        public Dictionary<TimeSpan, double> GetAMPMPeakGapOut(
            string signalId,
            DirectionTypes directionType,
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime)
        {
            Dictionary<TimeSpan, int> peaks = GetAMPMPeakFlowRate(
                signalId,
                directionType,
                startDate,
                endDate,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime);
            int phaseNumber = GetLTPhaseNumberByDirection(
                signalId,
                directionType);
            Dictionary <TimeSpan, double> averageTerminations = GetAverageTerminationsForPhase(
                signalId,
                phaseNumber,
                startDate,
                endDate,
                peaks);
            Dictionary <TimeSpan, double> averageGapOuts = GetAverageGapOutsForPhase(
                signalId,
                phaseNumber,
                startDate,
                endDate,
                amStartTime,
                peaks);
            return GetPercentageOfGapOuts(averageTerminations, averageGapOuts);
            
        }

        private Dictionary<TimeSpan, double> GetPercentageOfGapOuts(
            Dictionary<TimeSpan, double> averageTerminations,
            Dictionary<TimeSpan, double> averageGapOuts)
        {
            if(averageGapOuts.Values.Contains(0))
            {
                throw new ArithmeticException("Average Gap Out cannot be zero");
            }
            if(!averageGapOuts.Keys.Contains(averageGapOuts.Keys.Min()) ||
                !averageGapOuts.Keys.Contains(averageGapOuts.Keys.Max()))
            {
                throw new IndexOutOfRangeException("Peak hours do not align for AM/PM peak");
            }
            var amPeak = averageGapOuts.Keys.Min();
            var pmPeak = averageGapOuts.Keys.Max();
            Dictionary<TimeSpan, double> percentages = new Dictionary<TimeSpan, double>();
            percentages.Add(amPeak, averageGapOuts[amPeak] / averageTerminations[amPeak]);
            percentages.Add(pmPeak, averageGapOuts[pmPeak] / averageTerminations[pmPeak]);
            return percentages;
        }

        private Dictionary<TimeSpan, double> GetAverageGapOutsForPhase(
            string signalId,
            int phaseNumber,
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            Dictionary<TimeSpan, int> peaks)
        {
            Dictionary<TimeSpan, double> averages = new Dictionary<TimeSpan, double>();
            var amPeak = peaks.Min(p => p.Key);
            List<PhaseTerminationAggregation> amAggregations = new List<PhaseTerminationAggregation>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                amAggregations.AddRange(phaseTerminationAggregationRepository.GetPhaseTerminationsAggregationBySignalIdPhaseNumberAndDateRange(signalId, phaseNumber, tempDate.Date.Add(amPeak), tempDate.Date.Add(amPeak).AddHours(1)));
            }
            averages.Add(amPeak, amAggregations.Average(a => a.GapOuts));
            var pmPeak = peaks.Max(p => p.Key);
            List<PhaseTerminationAggregation> pmAggregations = new List<PhaseTerminationAggregation>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                pmAggregations.AddRange(phaseTerminationAggregationRepository.GetPhaseTerminationsAggregationBySignalIdPhaseNumberAndDateRange(signalId, phaseNumber, tempDate.Date.Add(amPeak), tempDate.Date.Add(amPeak).AddHours(1)));
            }
            averages.Add(amPeak, pmAggregations.Average(a => a.GapOuts));

            return averages;
        }
    

        private Dictionary<TimeSpan, double> GetAverageTerminationsForPhase(
            string signalId,
            int phaseNumber,
            DateTime startDate,
            DateTime endDate,
            Dictionary<TimeSpan, int> peaks)
        {
            Dictionary<TimeSpan, double> averages = new Dictionary<TimeSpan, double>();
            var amPeak = peaks.Min(p => p.Key);
            List<PhaseTerminationAggregation> amAggregations = new List<PhaseTerminationAggregation>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                amAggregations.AddRange(phaseTerminationAggregationRepository.GetPhaseTerminationsAggregationBySignalIdPhaseNumberAndDateRange(signalId, phaseNumber, tempDate.Date.Add(amPeak), tempDate.Date.Add(amPeak).AddHours(1)));               
            }
            averages.Add(amPeak, amAggregations.Average(a => a.ForceOffs + a.GapOuts + a.MaxOuts + a.Unknown));

            List<PhaseTerminationAggregation> pmAggregations = new List<PhaseTerminationAggregation>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                pmAggregations.AddRange(phaseTerminationAggregationRepository.GetPhaseTerminationsAggregationBySignalIdPhaseNumberAndDateRange(signalId, phaseNumber, tempDate.Date.Add(amPeak), tempDate.Date.Add(amPeak).AddHours(1)));
            }
            averages.Add(amPeak, pmAggregations.Average(a => a.ForceOffs + a.GapOuts + a.MaxOuts + a.Unknown));

            return averages;
        }

        private int GetLTPhaseNumberByDirection(
            string signalId,
            DirectionTypes directionType)
        {
            var detectors = GetLeftTurnDetectors(signalId, directionType);
            if(!detectors.Any())
            {
                throw new NotSupportedException("Detectors not found");
            }
            var approach = approachRepository.GetApproachByApproachID(detectors.First().ApproachId);
            return approach.PermissivePhaseNumber.HasValue ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber;
        }

        public Approach GetLTPhaseNumberPhaseTypeByDirection(
            string signalId,
            DirectionTypes directionType)
        {
            var detectors = GetLeftTurnDetectors(signalId, directionType);
            if (!detectors.Any())
            {
                throw new NotSupportedException("Detectors not found");
            }
            return approachRepository.GetApproachByApproachID(detectors.First().ApproachId);
           
        }

        public Dictionary<TimeSpan, int> GetAMPMPeakFlowRate(
            string signalId,
            DirectionTypes directionType,
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime)
        {
            if (!signalRepository.Exists(signalId))
            {
                throw new ArgumentException("Signal Not Found");
            }
            List<ATSPM.Data.Models.Detector> detectors = GetLeftTurnDetectors(signalId, directionType);
            if (!detectors.Any())
            {
                throw new NotSupportedException("No Detectors found");
            }
            List<DetectorEventCountAggregation> volumeAggregations = GetDetectorVolumebyDetector(
                detectors,
                startDate,
                endDate,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime);
            if (!volumeAggregations.Any())
            {
                throw new NotSupportedException("No Detector Activation Aggregations found");
            }
            List<TimeSpan> distinctTimeSpans = volumeAggregations.Select(v => v.BinStartTime.TimeOfDay).Distinct().OrderBy(v => v).ToList();

            Dictionary<TimeSpan, int> averageByBin = GetAveragesForBins(volumeAggregations, distinctTimeSpans);

            Dictionary<TimeSpan, int> hourlyFlowRates = GetHourlyFlowRates(distinctTimeSpans, averageByBin);

            return GetAmPmPeaks(amStartTime, amEndTime, pmStartTime, pmEndTime, hourlyFlowRates);
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

        public Dictionary<TimeSpan, int> GetAveragesForBins(
            List<DetectorEventCountAggregation> volumeAggregations,
            List<TimeSpan> distinctTimeSpans)
        {
            Dictionary<TimeSpan, int> averageByBin = new Dictionary<TimeSpan, int>();

            foreach (TimeSpan time in distinctTimeSpans)
            {
                int average = Convert.ToInt32(
                    Math.Round(volumeAggregations
                    .Where(v => v.BinStartTime.TimeOfDay == time)
                    .Average(v => v.EventCount)
                    ));
                averageByBin.Add(time, average);
            }

            return averageByBin;
        }

        public List<DetectorEventCountAggregation> GetDetectorVolumebyDetector(
            List<ATSPM.Data.Models.Detector> detectors,
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime)
        {
            var detectorAggregations = new List<DetectorEventCountAggregation>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                foreach (var detector in detectors)
                {
                    detectorAggregations.AddRange(detectorEventCountAggregationRepository
                        .GetDetectorEventCountAggregationByDetectorIdAndDateRange(detector.Id, tempDate.Add(amStartTime), tempDate.Add(amEndTime)));
                    detectorAggregations.AddRange(detectorEventCountAggregationRepository
                        .GetDetectorEventCountAggregationByDetectorIdAndDateRange(detector.Id, tempDate.Add(pmStartTime), tempDate.Add(pmEndTime)));
                }
            }
            return detectorAggregations;
        }

        public List<ATSPM.Data.Models.Detector> GetLeftTurnDetectors(string signalId, DirectionTypes directionType)
        {
            return detectorRepository.GetDetectorsBySignalIdMovementTypeIdDirectionTypeId(signalId, directionType, new List<MovementTypes>() { MovementTypes.L, MovementTypes.TL}).ToList();
        }
    }
}
