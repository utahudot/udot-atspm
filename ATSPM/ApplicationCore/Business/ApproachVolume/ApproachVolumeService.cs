using ATSPM.Application.Business.Common;
using ATSPM.Application.Extensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ATSPM.Application.Business.ApproachVolume
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
            Location Location,
            List<ControllerEventLog> primaryDetectorEvents,
            List<ControllerEventLog> opposingDetectorEvents,
            List<Approach> primaryApproaches,
            List<Approach> opposingApproaches,
            int distanceFromStopBar,
            DetectionType detectionType
            )
        {
            int binSizeMultiplier = 60 / options.BinSize;
            var primaryDirectionVolume = new VolumeCollection(
                options.Start,
                options.End,
                primaryDetectorEvents,
                options.BinSize);
            var opposingDirectionVolume = new VolumeCollection(
                options.Start,
                options.End,
                opposingDetectorEvents,
                options.BinSize);
            var combinedDirectionsVolumes = new VolumeCollection(primaryDirectionVolume, opposingDirectionVolume, options.BinSize);
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
            double primaryDirectionPeakHourDFactor = GetPeakHourDFactor(primayDirectionPeakHourItem.Key, primayDirectionPeakHourItem.Value, opposingDirectionVolume);
            string primaryDirectionPeakHourString = primayDirectionPeakHourItem.Key.ToShortTimeString() + " - " + primayDirectionPeakHourItem.Key.AddHours(1).ToShortTimeString();
            int opposingDirectionPeakValueInHour = FindPeakValueinHour(opposingDirectionPeakHourItem.Key, opposingDirectionVolume, binSizeMultiplier);
            int opposingDirectionPeakHourlyValueInHour = opposingDirectionPeakValueInHour * binSizeMultiplier;

            double opposingDirectionPeakHourFactor = GetPeakHourFactor(opposingDirectionPeakHourItem.Value, opposingDirectionPeakHourlyValueInHour);
            double opposingDirectionPeakHourDFactor = GetPeakHourDFactor(opposingDirectionPeakHourItem.Key, opposingDirectionPeakHourItem.Value, primaryDirectionVolume);
            string opposingDirectionPeakHourString = opposingDirectionPeakHourItem.Key.ToShortTimeString() + " - " + opposingDirectionPeakHourItem.Key.AddHours(1).ToShortTimeString();
            var detector = primaryApproaches.First().GetAllDetectorsOfDetectionType(detectionType).FirstOrDefault();

            return new ApproachVolumeResult(
                options.locationIdentifier,
                options.Start,
                options.End,
                detectionType.Id.GetDisplayAttribute()?.Name,
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
                combinedPeakHourItem.Value,
                combinedPeakHourFactor,
                combinedVolume,
                primaryDirectionPeakHourString,
                primaryKFactor,
                primayDirectionPeakHourItem.Value,
                primaryDirectionPeakHourFactor,
                Convert.ToInt32(primaryDirectionTotalVolume),
                primaryDirectionPeakHourDFactor,
                opposingDirectionPeakHourString,
                opposingKFactor,
                opposingDirectionPeakHourItem.Value,
                opposingDirectionPeakHourFactor,
                Convert.ToInt32(opposingDirectionTotalVolume),
                opposingDirectionPeakHourDFactor
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

        public static DirectionTypes GetOpposingDirection(DirectionTypes direction)
        {
            var opposingDirection = direction;
            switch (direction)
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

        private List<DataPointForDouble> GetDFactorSeries(VolumeCollection approachVolume, VolumeCollection combinedVolume)
        {
            List<DataPointForDouble> result = new List<DataPointForDouble>();
            for (int i = 0; i < approachVolume.Items.Count; i++)
            {
                if (combinedVolume.Items[i].DetectorCount == 0)
                {
                    result.Add(new DataPointForDouble(approachVolume.Items[i].StartTime, 0));
                }
                else
                {
                    result.Add(new DataPointForDouble(approachVolume.Items[i].StartTime, Convert.ToDouble(approachVolume.Items[i].DetectorCount) / Convert.ToDouble(combinedVolume.Items[i].DetectorCount)));
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
                var index = direction2Volumes.FindIndex(d => d.Timestamp == current.Timestamp);
                if (index >= 0)
                    sortedDictionary.Add(current.Timestamp, direction2Volumes[index].Value + current.Value);
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

        protected double GetPeakHourDFactor(DateTime startofHour, int peakhourvolume, VolumeCollection volumes)
        {
            int totalVolume = 0;
            double PHDF = 0;
            if (volumes.Items.Any(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1)))
            {
                totalVolume = volumes.Items
                    .Where(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1))
                    .Sum(v => v.DetectorCount);
            }
            //totalVolume /= binMultiplier;
            totalVolume += peakhourvolume;
            if (totalVolume > 0)
                PHDF = Round(Convert.ToDouble(peakhourvolume) / Convert.ToDouble(totalVolume), 3);
            else
                PHDF = 0;
            return PHDF;
        }
    }
}