﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.ApproachVolume/ApproachVolumeService.cs
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

using System.Data;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Business.ApproachVolume
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
            List<IndianaEvent> primaryDetectorEvents,
            List<IndianaEvent> opposingDetectorEvents,
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
            var direction1VolumesSeries = GetDirectionSeries(primaryDirectionVolume);
            var direction2VolumesSeries = GetDirectionSeries(opposingDirectionVolume);
            var combinedDirectionsVolumesSeries = GetDirectionSeries(combinedDirectionsVolumes);
            //var combinedDirectionVolumeDictionary = CombineDirectionHourlyVolumes(direction1VolumesSeries, direction2VolumesSeries);

            var primaryDirectionTotalVolume = Convert.ToDouble(primaryDirectionVolume.Items.Sum(d => d.DetectorCount));
            var opposingDirectionTotalVolume = Convert.ToDouble(opposingDirectionVolume.Items.Sum(d => d.DetectorCount));

            KeyValuePair<DateTime, int> primayDirectionPeakHourItem = GetPeakHourVolumeItem(primaryDirectionVolume, binSizeMultiplier);
            KeyValuePair<DateTime, int> opposingDirectionPeakHourItem = GetPeakHourVolumeItem(opposingDirectionVolume, binSizeMultiplier);

            //This could be different than the opposing peak hour start so it needs to be calculated here
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

            return new ApproachVolumeResult(
                options.LocationIdentifier,
                options.Start,
                options.End,
                detectionType.Id.GetDisplayAttribute()?.Name,
                distanceFromStopBar,
                primaryApproaches[0].DirectionType.Description,
                opposingApproaches[0].DirectionType.Description)
            {
                PrimaryDirectionVolumes = direction1VolumesSeries,
                OpposingDirectionVolumes = direction2VolumesSeries,
                CombinedDirectionVolumes = combinedDirectionsVolumesSeries,
                PrimaryDFactors = GetDFactorSeries(primaryDirectionVolume, combinedDirectionsVolumes),
                OpposingDFactors = GetDFactorSeries(opposingDirectionVolume, combinedDirectionsVolumes),
                SummaryData = new SummaryData
                {
                    PeakHour = combinedPeakHourString,
                    KFactor = combinedPeakHourKFactor,
                    PeakHourVolume = combinedPeakHourItem.Value,
                    PeakHourFactor = combinedPeakHourFactor,
                    TotalVolume = combinedVolume,
                    PrimaryPeakHour = primaryDirectionPeakHourString,
                    PrimaryKFactor = primaryKFactor,
                    PrimaryPeakHourVolume = primayDirectionPeakHourItem.Value,
                    PrimaryPeakHourFactor = primaryDirectionPeakHourFactor,
                    PrimaryTotalVolume = Convert.ToInt32(primaryDirectionTotalVolume),
                    PrimaryDFactor = primaryDirectionPeakHourDFactor,
                    OpposingPeakHour = opposingDirectionPeakHourString,
                    OpposingKFactor = opposingKFactor,
                    OpposingPeakHourVolume = opposingDirectionPeakHourItem.Value,
                    OpposingPeakHourFactor = opposingDirectionPeakHourFactor,
                    OpposingTotalVolume = Convert.ToInt32(opposingDirectionTotalVolume),
                    OpposingDFactor = opposingDirectionPeakHourDFactor
                }

            };
        }

        private static double GetKFactor(int primaryDirectionPeakHourVolume, int opposingVolumeForPrimaryPeakHour, double primaryDirectionTotalVolume, double opposingDirectionTotalVolume)
        {
            return (primaryDirectionPeakHourVolume + opposingVolumeForPrimaryPeakHour) / (primaryDirectionTotalVolume + opposingDirectionTotalVolume);
        }

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
            }
            return opposingDirection;
        }

        private static List<DataPointForDouble> GetDFactorSeries(VolumeCollection approachVolume, VolumeCollection combinedVolume)
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

        private static List<DataPointForInt> GetDirectionSeries(VolumeCollection approachVolume)
        {
            if (approachVolume != null && approachVolume.Items.Any())
            {
                return approachVolume.Items.ConvertAll(x => new DataPointForInt(x.StartTime, x.HourlyVolume));
            }
            return new List<DataPointForInt>();
        }

        private static double GetPeakHourFactor(int direction1PeakHourVolume, int peakHourMaxHourlyVolume)
        {
            double peakHourFactor = 0;
            if (peakHourMaxHourlyVolume > 0)
            {
                peakHourFactor = Convert.ToDouble(direction1PeakHourVolume) / Convert.ToDouble(peakHourMaxHourlyVolume);
            }
            return peakHourFactor;
        }

        private static SortedDictionary<DateTime, int> CombineDirectionHourlyVolumes(List<DataPointForInt> direction1Volumes, List<DataPointForInt> direction2Volumes)
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

        private static void SetStartTimeAndEndTime(VolumeCollection direction1Volumes, VolumeCollection direction2Volumes, out DateTime startTime, out DateTime endTime)
        {
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
            else
            {
                startTime = new DateTime();
                endTime = new DateTime();
            }
        }

        protected static KeyValuePair<DateTime, int> GetPeakHourVolumeItem(VolumeCollection volumes,
            int binMultiplier)
        {
            KeyValuePair<DateTime, int> peakHourValue = new KeyValuePair<DateTime, int>();
            SortedDictionary<DateTime, int> iteratedVolumes = new SortedDictionary<DateTime, int>();
            foreach (var start in volumes.Items.Select(i => i.StartTime))
            {
                iteratedVolumes.Add(start, volumes.Items.Where(v => v.StartTime >= start && v.StartTime < start.AddHours(1)).Sum(v => v.DetectorCount));
            }
            peakHourValue = iteratedVolumes.OrderByDescending(i => i.Value).FirstOrDefault();

            return peakHourValue;
        }

        protected static int FindPeakValueinHour(DateTime StartofHour, VolumeCollection volDic,
            int binMultiplier)
        {
            int maxVolume = 0;

            for (int i = 0; i < binMultiplier; i++)
            {
                if (volDic.Items.Exists(d => d.StartTime == StartofHour))
                {
                    var detectorCount = volDic.Items.Find(i => i.StartTime == StartofHour).DetectorCount;
                    if (maxVolume < detectorCount)
                        maxVolume = detectorCount;
                }

                StartofHour = StartofHour.AddMinutes(60 / (double)binMultiplier);
            }
            return maxVolume;
        }

        protected static double GetPeakHourDFactor(DateTime startofHour, int peakhourvolume, VolumeCollection volumes)
        {
            int totalVolume = 0;
            double PHDF = 0;
            if (volumes.Items.Exists(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1)))
            {
                totalVolume = volumes.Items
                    .Where(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1))
                    .Sum(v => v.DetectorCount);
            }
            totalVolume += peakhourvolume;
            if (totalVolume > 0)
                PHDF = Round(Convert.ToDouble(peakhourvolume) / Convert.ToDouble(totalVolume), 3);
            else
                PHDF = 0;
            return PHDF;
        }
    }
}