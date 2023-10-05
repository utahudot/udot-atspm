using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Reports.Business.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ATSPM.Application.Reports.Business.ApproachVolume
{
    public class ApproachVolumeService
    {
        public ApproachVolumeOptions Options { get; private set; }

        public ApproachVolumeService()
        {
        }

        public static double Round(double d, int digits)

        {
            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }

        public ApproachVolumeResult GetChartData(
            ApproachVolumeOptions options,
            Signal signal,
            List<ControllerEventLog> primaryDetectorEvents,
            List<ControllerEventLog> opposingDetectorEvents,
            int distanceFromStopBar
            )
        {
            int binSizeMultiplier = 60 / options.SelectedBinSize;
            DirectionTypes opposingDirection = GetOpposingDirection(options);
            var primaryApproaches = signal.Approaches.Where(a => a.DirectionTypeId == options.Direction).ToList();
            var opposingApproaches = signal.Approaches.Where(a => a.DirectionTypeId == opposingDirection).ToList();
            var primaryDirectionVolume = new VolumeCollection(
                options.Start,
                options.End,
                primaryDetectorEvents,
                options.SelectedBinSize);
            var opposingDirectionVolume = new VolumeCollection(
                options.Start,
                options.End,
                opposingDetectorEvents,
                options.SelectedBinSize);
            var combinedDirectionsVolumes = new VolumeCollection(primaryDirectionVolume, opposingDirectionVolume, options.SelectedBinSize);
            //ApproachVolume approachVolume = new ApproachVolume(
            //    primaryApproaches,
            //    opposingApproaches,
            //    options,
            //    opposingDirection,
            //    options.DetectionType,
            //    detectionTypeRepository,
            //    controllerEventLogRepository);
            var direction1VolumesSeries = GetDirectionSeries(primaryDirectionVolume);
            var direction2VolumesSeries = GetDirectionSeries(opposingDirectionVolume);
            var combinedDirectionsVolumesSeries = GetDirectionSeries(combinedDirectionsVolumes);
            var combinedDirectionVolumeDictionary = CombineDirectionHourlyVolumes(direction1VolumesSeries, direction2VolumesSeries);

            var primaryDirectionTotalVolume = Convert.ToDouble(primaryDirectionVolume.Items.Sum(d => d.DetectorCount));
            var opposingDirectionTotalVolume = Convert.ToDouble(opposingDirectionVolume.Items.Sum(d => d.DetectorCount));

            KeyValuePair<DateTime, int> primayDirectionPeakHourItem = GetPeakHourVolumeItem(primaryDirectionVolume, binSizeMultiplier);
            KeyValuePair<DateTime, int> opposingDirectionPeakHourItem = GetPeakHourVolumeItem(opposingDirectionVolume, binSizeMultiplier);

            //This could be different than the opposing peak hour volume so it needs to be calculated here
            var opposingVolumeForPrimaryPeakHour = opposingDirectionVolume.Items.Where(d => d.StartTime >= primayDirectionPeakHourItem.Key && d.StartTime < primayDirectionPeakHourItem.Key.AddHours(1)).Sum(d => d.DetectorCount);
            var primaryVolumeForOpposingPeakHour = primaryDirectionVolume.Items.Where(d => d.StartTime >= opposingDirectionPeakHourItem.Key && d.StartTime < opposingDirectionPeakHourItem.Key.AddHours(1)).Sum(d => d.DetectorCount);

            var primaryKFactor = GetKFactor(primayDirectionPeakHourItem.Value, opposingVolumeForPrimaryPeakHour, primaryDirectionTotalVolume, opposingDirectionTotalVolume);
            var opposingKFactor = GetKFactor(opposingDirectionPeakHourItem.Value, primaryVolumeForOpposingPeakHour, opposingDirectionTotalVolume, primaryDirectionTotalVolume);


            int primaryDirectionPeakHourValue = FindPeakValueinHour(primayDirectionPeakHourItem.Key, primaryDirectionVolume, binSizeMultiplier);
            int primaryDirectionPeakHourlyValueInHour = primaryDirectionPeakHourValue * binSizeMultiplier;

            DateTime startTime, endTime;
            SetStartTimeAndEndTime(primaryDirectionVolume, opposingDirectionVolume, out startTime, out endTime);
            KeyValuePair<DateTime, int> combinedPeakHourItem = GetPeakHourVolumeItem(combinedDirectionsVolumes, binSizeMultiplier);
            int combinedVolume = combinedDirectionsVolumes.Items.Sum(d => d.DetectorCount);
            int combinedPeakHourValue = FindPeakValueinHour(combinedPeakHourItem.Key, combinedDirectionsVolumes, binSizeMultiplier);
            double combinedPeakHourFactor = GetPeakHourFactor(combinedPeakHourItem.Value, combinedPeakHourValue * binSizeMultiplier);
            double combinedPeakHourKFactor = Convert.ToDouble(combinedPeakHourItem.Value) / Convert.ToDouble(combinedVolume);
            string combinedPeakHourString = combinedPeakHourItem.Key.ToShortTimeString() + " - " + combinedPeakHourItem.Key.AddHours(1).ToShortTimeString();

            double primaryDirectionPeakHourFactor = GetPeakHourFactor(primayDirectionPeakHourItem.Value, primaryDirectionPeakHourlyValueInHour);
            double primaryDirectionPeakHourDFactor = GetPeakHourDFactor(primayDirectionPeakHourItem.Key, primayDirectionPeakHourItem.Value, opposingDirectionVolume, binSizeMultiplier);
            string primaryDirectionPeakHourString = primayDirectionPeakHourItem.Key.ToShortTimeString() + " - " + primayDirectionPeakHourItem.Key.AddHours(1).ToShortTimeString();
            int opposingDirectionPeakValueInHour = FindPeakValueinHour(opposingDirectionPeakHourItem.Key, opposingDirectionVolume, binSizeMultiplier);
            int opposingDirectionPeakHourlyValueInHour = opposingDirectionPeakValueInHour * binSizeMultiplier;

            double opposingDirectionPeakHourFactor = GetPeakHourFactor(opposingDirectionPeakHourItem.Value, opposingDirectionPeakHourlyValueInHour);
            double opposingDirectionPeakHourDFactor = GetPeakHourDFactor(opposingDirectionPeakHourItem.Key, opposingDirectionPeakHourItem.Value, primaryDirectionVolume, binSizeMultiplier);
            string opposingDirectionPeakHourString = opposingDirectionPeakHourItem.Key.ToShortTimeString() + " - " + opposingDirectionPeakHourItem.Key.AddHours(1).ToShortTimeString();
            var detector = primaryApproaches.First().GetAllDetectorsOfDetectionType(options.DetectionType).FirstOrDefault();

            return new ApproachVolumeResult(
                options.SignalIdentifier,
                options.Start,
                options.End,
                options.DetectionType.GetDisplayName(),
                distanceFromStopBar,
                primaryApproaches.First().DirectionType.Description,
                direction1VolumesSeries,
                opposingApproaches.First().DirectionType.Description,
                direction2VolumesSeries,
                combinedDirectionsVolumesSeries,
                GetDFactorSeries(primaryDirectionVolume, combinedDirectionsVolumes),
                GetDFactorSeries(opposingDirectionVolume, combinedDirectionsVolumes),
                combinedPeakHourString,
                combinedPeakHourKFactor,
                combinedPeakHourValue,
                combinedPeakHourFactor,
                combinedVolume,
                primaryDirectionPeakHourString,
                primaryKFactor,
                primayDirectionPeakHourItem.Value,
                primaryDirectionPeakHourFactor,
                Convert.ToInt32(primaryDirectionTotalVolume),
                opposingDirectionPeakHourString,
                opposingKFactor,
                opposingDirectionPeakHourItem.Value,
                opposingDirectionPeakHourFactor,
                Convert.ToInt32(opposingDirectionTotalVolume)
                );
        }

        private double GetKFactor(int primaryDirectionPeakHourVolume, int opposingVolumeForPrimaryPeakHour, double primaryDirectionTotalVolume, double opposingDirectionTotalVolume)
        {
            return (primaryDirectionPeakHourVolume + opposingVolumeForPrimaryPeakHour) / (primaryDirectionTotalVolume + opposingDirectionTotalVolume);
        }

        //private VolumeCollection GetVolumeByDetection(
        //   List<Approach> approaches,
        //   ApproachVolumeOptions options,
        //   List<ControllerEventLog> detectorEvents)
        //{
        //    var detectors = approaches.SelectMany(a => a.GetDetectorsForMetricType(7))
        //        .Where(d => d.LaneTypeId == LaneTypes.V).ToList();


        //    return new VolumeCollection(
        //        options.Start,
        //        options.End,
        //        detectorEvents,
        //        options.SelectedBinSize);
        //}

        public static DirectionTypes GetOpposingDirection(ApproachVolumeOptions options)
        {
            var opposingDirection = options.Direction;
            switch (options.Direction)
            {
                case DirectionTypes.EB:
                    opposingDirection = DirectionTypes.WB;
                    break;
                case DirectionTypes.WB:
                    opposingDirection = DirectionTypes.EB;
                    break;
                case DirectionTypes.NB:
                    opposingDirection = DirectionTypes.SB;
                    break;
                case DirectionTypes.SB:
                    opposingDirection = DirectionTypes.NB;
                    break;
                case DirectionTypes.NE:
                    opposingDirection = DirectionTypes.SW;
                    break;
                case DirectionTypes.NW:
                    opposingDirection = DirectionTypes.SE;
                    break;
                case DirectionTypes.SE:
                    opposingDirection = DirectionTypes.NW;
                    break;
                case DirectionTypes.SW:
                    opposingDirection = DirectionTypes.NE;
                    break;
            };
            return opposingDirection;
        }

        private List<DFactors> GetDFactorSeries(VolumeCollection approachVolume, VolumeCollection combinedVolume)
        {
            List<DFactors> result = new List<DFactors>();
            for (int i = 0; i < approachVolume.Items.Count; i++)
            {
                if (combinedVolume.Items[i].DetectorCount == 0)
                {
                    result.Add(new DFactors(approachVolume.Items[i].StartTime, 0));
                }
                else
                {
                    result.Add(new DFactors(approachVolume.Items[i].StartTime, Convert.ToDouble(approachVolume.Items[i].DetectorCount) / Convert.ToDouble(combinedVolume.Items[i].DetectorCount)));
                }
            }
            return result;
        }

        private List<DataPointForInt> GetDirectionSeries(VolumeCollection approachVolume)
        {
            if (approachVolume != null && approachVolume.Items.Any())
            {
                return approachVolume.Items.ConvertAll(x => new DataPointForInt(x.StartTime, x.HourlyVolume));
            }
            return new List<DataPointForInt>();
        }



        private static double GetPeakHourFactor(int direction1PeakHourVolume, int PeakHourMaxHourlyVolume)
        {
            double PeakHourFactor = 0;
            if (PeakHourMaxHourlyVolume > 0)
            {
                PeakHourFactor = Convert.ToDouble(direction1PeakHourVolume) / Convert.ToDouble(PeakHourMaxHourlyVolume);
            }
            return PeakHourFactor;
        }

        private SortedDictionary<DateTime, int> CombineDirectionHourlyVolumes(List<DataPointForInt> direction1Volumes, List<DataPointForInt> direction2Volumes)
        {
            var sortedDictionary = new SortedDictionary<DateTime, int>();
            foreach (DataPointForInt current in direction1Volumes)
            {
                var index = direction2Volumes.FindIndex(d => d.TimeStamp == current.TimeStamp);
                if (index >= 0)
                    sortedDictionary.Add(current.TimeStamp, direction2Volumes[index].Value + current.Value);
            }
            return sortedDictionary;
        }

        //private static bool IsValidTimePeriodForKFactors(DateTime startTime, DateTime endTime)
        //{
        //    TimeSpan timeDiff = endTime.Subtract(startTime);
        //    bool validKfactors = timeDiff.TotalHours >= 23 && timeDiff.TotalHours < 25;
        //    return validKfactors;
        //}

        private static void SetStartTimeAndEndTime(VolumeCollection direction1Volumes, VolumeCollection direction2Volumes, out DateTime startTime, out DateTime endTime)
        {
            startTime = new DateTime();
            endTime = new DateTime();
            //Create the Volume Metrics table
            if (direction1Volumes.Items.Count > 0)
            {
                startTime = direction1Volumes.Items.Min(d => d.StartTime);
                endTime = direction1Volumes.Items.Max(d => d.StartTime);
            }
            else if (direction2Volumes.Items.Count > 0)
            {
                startTime = direction2Volumes.Items.Min(d => d.StartTime);
                endTime = direction2Volumes.Items.Max(d => d.StartTime);
            }
        }

        protected KeyValuePair<DateTime, int> GetPeakHourVolumeItem(VolumeCollection volumes,
            int binMultiplier)
        {
            KeyValuePair<DateTime, int> peakHourValue = new KeyValuePair<DateTime, int>();
            SortedDictionary<DateTime, int> iteratedVolumes = new SortedDictionary<DateTime, int>();
            foreach (var volume in volumes.Items)
            {
                iteratedVolumes.Add(volume.StartTime, volumes.Items.Where(v => v.StartTime >= volume.StartTime && v.StartTime < volume.StartTime.AddHours(1)).Sum(v => v.DetectorCount));
            }
            peakHourValue = iteratedVolumes.OrderByDescending(i => i.Value).FirstOrDefault();

            return peakHourValue;
        }

        protected int FindPeakValueinHour(DateTime StartofHour, VolumeCollection volDic,
            int binMultiplier)
        {
            int maxVolume = 0;

            for (int i = 0; i < binMultiplier; i++)
            {
                if (volDic.Items.Any(d => d.StartTime == StartofHour))
                    if (maxVolume < volDic.Items.FirstOrDefault(i => i.StartTime == StartofHour).DetectorCount)
                        maxVolume = volDic.Items.FirstOrDefault(i => i.StartTime == StartofHour).DetectorCount;

                StartofHour = StartofHour.AddMinutes(60 / binMultiplier);
            }
            return maxVolume;
        }

        protected double GetPeakHourDFactor(DateTime StartofHour, int Peakhourvolume, VolumeCollection volDic,
            int binMultiplier)
        {
            int totalVolume = 0;
            double PHDF = 0.0;

            for (int i = 0; i < binMultiplier; i++)
            {
                if (volDic.Items.Any(d => d.StartTime == StartofHour))
                    totalVolume = totalVolume + volDic.Items.FirstOrDefault(d => d.StartTime == StartofHour).DetectorCount;

                StartofHour = StartofHour.AddMinutes(60 / binMultiplier);
            }
            totalVolume /= binMultiplier;
            totalVolume += Peakhourvolume;
            if (totalVolume > 0)
                PHDF = Round(Convert.ToDouble(Peakhourvolume) / Convert.ToDouble(totalVolume), 3);
            else
                PHDF = 0.0;
            return PHDF;
        }
    }
}