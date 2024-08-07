﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.TurningMovementCounts/TurningMovementCountsService.cs
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

using System.ComponentModel.DataAnnotations;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Business.TurningMovementCounts
{
    public class TMCData
    {
        public string Direction { get; set; }
        public string MovementType { get; set; }
        public string LaneType { get; set; }
        public DateTime Timestamp { get; set; }
        public int Count { get; set; }
    }
    public class TurningMovementCountsService
    {
        public TurningMovementCountsService()
        {
        }

        public async Task<TurningMovementCountsLanesResult> GetChartData(
            List<Detector> detectorsByDirection,
            LaneTypes laneType,
            MovementTypes movementType,
            DirectionTypes directionType,
            TurningMovementCountsOptions options,
            List<IndianaEvent> detectorEvents,
            List<Plan> plans,
            string locationIdentifier,
            string LocationDescription)
        {
            //var plans = planService.GetBasicPlans(options.Start, options.End, LocationIdentifier, plans);
            var tmcDetectors = new List<Detector>();
            FindLaneDetectors(tmcDetectors, movementType, detectorsByDirection, laneType);

            if (tmcDetectors.Count == 0)
                return null;

            var laneVolumes = GetVolumeDictionaryByDetector(tmcDetectors, options.Start, options.End, detectorEvents, options.BinSize);
            var allLanesMovementVolumes = new VolumeCollection(laneVolumes.Values.ToList(), options.BinSize);
            var laneNumberVolumes = new Dictionary<int, VolumeCollection>();
            var lanes = new List<Lane>();

            foreach (var laneNumber in tmcDetectors.Select(d => d.LaneNumber).Distinct())
            {
                var volumes = laneVolumes.Where(l => l.Key.LaneNumber == laneNumber).ToList();
                var laneVolume = new VolumeCollection(volumes.Select(l => l.Value).ToList(), options.BinSize);
                var firstDetector = volumes.FirstOrDefault();

                lanes.Add(new Lane
                {
                    LaneNumber = laneNumber,
                    MovementType = firstDetector.Key?.MovementType.GetDisplayName(),
                    LaneType = firstDetector.Key?.LaneType ?? 0,
                    Volume = laneVolume.Items.Select(i => new DataPointForInt(i.StartTime, i.HourlyVolume)).ToList()
                });

                laneNumberVolumes.Add(laneNumber.Value, laneVolume);
            }

            var highestDetectorCountByLane = laneNumberVolumes.Values.Max(l => l.TotalDetectorCounts);
            var totalDetectorCounts = allLanesMovementVolumes.TotalDetectorCounts;
            var flu = totalDetectorCounts / (tmcDetectors.Count * (double)highestDetectorCountByLane);

            var peakHour = GetPeakHour(allLanesMovementVolumes, 60 / options.BinSize);
            var peakHourEnd = peakHour.Key.AddHours(1);
            var binMultiplier = 60 / options.BinSize;

            var peakHourMaxVolume = allLanesMovementVolumes.Items
                .Where(i => i.StartTime >= peakHour.Key && i.StartTime < peakHourEnd)
                .Max(i => i.HourlyVolume);

            var peakHourFactor = GetPeakHourFactor(peakHour.Value, peakHourMaxVolume, binMultiplier);

            var peakHourDetectorCount = allLanesMovementVolumes.Items
                .Where(i => i.StartTime >= peakHour.Key && i.StartTime < peakHourEnd)
                .Sum(i => i.DetectorCount);

            return new TurningMovementCountsLanesResult(
                locationIdentifier,
                LocationDescription,
                options.Start,
                options.End,
                directionType.GetAttributeOfType<DisplayAttribute>().Name,
                laneType.GetAttributeOfType<DisplayAttribute>().Name,
                movementType.GetAttributeOfType<DisplayAttribute>().Name,
                plans,
                lanes,
                allLanesMovementVolumes.Items.Select(i => new DataPointForInt(i.StartTime, i.HourlyVolume)).ToList(),
                allLanesMovementVolumes.Items.Select(i => new DataPointForInt(i.StartTime, i.DetectorCount)).ToList(),
                totalDetectorCounts,
                $"{peakHour.Key.ToShortTimeString()} - {peakHourEnd.ToShortTimeString()}",
                peakHour.Value / binMultiplier,
                peakHourFactor,
                flu
            );
        }




        private Dictionary<Detector, VolumeCollection> GetVolumeDictionaryByDetector(
            List<Detector> tmcDetectors,
            DateTime start,
            DateTime end,
            List<IndianaEvent> detectorEvents,
            int binSize)
        {
            var laneVolumes = new Dictionary<Detector, VolumeCollection>();
            foreach (var detector in tmcDetectors)
            {
                laneVolumes.Add(detector, new VolumeCollection(start, end, detectorEvents.Where(e => e.EventCode == 82 && e.EventParam == detector.DetectorChannel).ToList(), binSize));
            }
            return laneVolumes;
        }


        private void FindLaneDetectors(List<Detector> tmcDetectors, MovementTypes movementType,
            List<Detector> detectorsByDirection, LaneTypes laneType)
        {
            foreach (var detector in detectorsByDirection)
                if (detector.LaneType == laneType)
                    if ((int)movementType == 1)
                    {
                        if (detector.MovementType == MovementTypes.T ||
                            detector.MovementType == MovementTypes.TR ||
                            detector.MovementType == MovementTypes.TL)
                            tmcDetectors.Add(detector);
                    }
                    else if (detector.MovementType == movementType)
                    {
                        tmcDetectors.Add(detector);
                    }
        }


        public double? GetPeakHourFactor(int PHV, int PeakHourMAXVolume, int binMultiplier)
        {
            if (PeakHourMAXVolume > 0)
            {
                return SetSigFigs(
                    Convert.ToDouble(PHV) / (Convert.ToDouble(PeakHourMAXVolume) * Convert.ToDouble(binMultiplier)), 2);
            }
            else
            {
                return null;
            }
        }

        public KeyValuePair<DateTime, int> GetPeakHour(VolumeCollection volumeCollection, int binMultiplier)
        {
            var subTotal = 0;
            var peakHourValue = new KeyValuePair<DateTime, int>();

            var startTime = new DateTime();
            var iteratedVolumes = new SortedDictionary<DateTime, int>();

            for (var i = 0; i < volumeCollection.Items.Count - (binMultiplier - 1); i++)
            {
                startTime = volumeCollection.Items.ElementAt(i).StartTime;
                subTotal = 0;
                for (var x = 0; x < binMultiplier; x++)
                    subTotal = subTotal + volumeCollection.Items.ElementAt(i + x).HourlyVolume;
                iteratedVolumes.Add(startTime, subTotal);
            }

            //Find the highest value in the iterated Volumes dictionary.
            //This should bee the peak hour.
            foreach (var kvp in iteratedVolumes)
                if (kvp.Value > peakHourValue.Value)
                    peakHourValue = kvp;

            return peakHourValue;
        }


        public double SetSigFigs(double d, int digits)
        {
            var scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);

            return scale * Math.Round(d / scale, digits);
        }
    }
}
