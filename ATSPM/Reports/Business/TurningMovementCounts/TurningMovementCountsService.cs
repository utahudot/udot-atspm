using ATSPM.Application.Reports.Business.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Application.Reports.Business.TurningMovementCounts
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
        private readonly PlanService planService;
        public TurningMovementCountsService(PlanService planService)
        {
            this.planService = planService;
        }

        public async Task<TurningMovementCountsResult> GetChartData(
            List<Detector> detectorsByDirection,
            LaneTypes laneType,
            MovementTypes movementType,
            DirectionTypes directionType,
            TurningMovementCountsOptions options,
            List<ControllerEventLog> detectorEvents,
            List<Plan> plans,
            string signalIdentifier,
            string signalDescription)
        {
            //var plans = planService.GetBasicPlans(options.Start, options.End, signalIdentifier, plans);
            var tmcDetectors = new List<Detector>();
            FindLaneDetectors(tmcDetectors, movementType, detectorsByDirection, laneType);

            if (tmcDetectors.Count == 0)
                return null;

            var laneVolumes = GetVolumeDictionaryByDetector(tmcDetectors, options.Start, options.End, detectorEvents, options.SelectedBinSize);
            var allLanesMovementVolumes = new VolumeCollection(laneVolumes.Values.ToList(), options.SelectedBinSize);
            var laneNumberVolumes = new Dictionary<int, VolumeCollection>();
            var lanes = new List<Lane>();

            foreach (var laneNumber in tmcDetectors.Select(d => d.LaneNumber).Distinct())
            {
                var volumes = laneVolumes.Where(l => l.Key.LaneNumber == laneNumber).ToList();
                var laneVolume = new VolumeCollection(volumes.Select(l => l.Value).ToList(), options.SelectedBinSize);
                var firstDetector = volumes.FirstOrDefault();

                lanes.Add(new Lane
                {
                    LaneNumber = laneNumber,
                    MovementType = firstDetector.Key?.MovementTypeId ?? 0,
                    LaneType = firstDetector.Key?.LaneTypeId ?? 0,
                    Volume = laneVolume.Items.Select(i => new LaneVolume { StartTime = i.StartTime, Volume = i.HourlyVolume }).ToList()
                });

                laneNumberVolumes.Add(laneNumber.Value, laneVolume);
            }

            var highestDetectorCountByLane = laneNumberVolumes.Values.Max(l => l.TotalDetectorCounts);
            var totalDetectorCounts = allLanesMovementVolumes.TotalDetectorCounts;
            var flu = totalDetectorCounts / ((double)tmcDetectors.Count * (double)highestDetectorCountByLane);

            var peakHour = GetPeakHour(allLanesMovementVolumes, 60 / options.SelectedBinSize);
            var peakHourEnd = peakHour.Key.AddHours(1);
            var binMultiplier = 60 / options.SelectedBinSize;

            var peakHourMaxVolume = allLanesMovementVolumes.Items
                .Where(i => i.StartTime >= peakHour.Key && i.StartTime < peakHourEnd)
                .Max(i => i.HourlyVolume);

            var peakHourFactor = GetPeakHourFactor(peakHour.Value, peakHourMaxVolume, binMultiplier);

            var peakHourDetectorCount = allLanesMovementVolumes.Items
                .Where(i => i.StartTime >= peakHour.Key && i.StartTime < peakHourEnd)
                .Sum(i => i.DetectorCount);

            return new TurningMovementCountsResult(
                signalIdentifier,
                signalDescription,
                options.Start,
                options.End,
                directionType.ToString(),
                Enum.GetName(laneType),
                Enum.GetName(movementType),
                plans,
                lanes,
                allLanesMovementVolumes.Items.Select(i => new TotalVolume(i.StartTime, i.HourlyVolume)).ToList(),
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
            List<ControllerEventLog> detectorEvents,
            int binSize)
        {
            var laneVolumes = new Dictionary<Detector, VolumeCollection>();
            foreach (var detector in tmcDetectors)
            {
                laneVolumes.Add(detector, new VolumeCollection(start, end, detectorEvents.Where(e => e.EventCode == 82 && e.EventParam == detector.DetChannel).ToList(), binSize));
            }
            return laneVolumes;
        }


        private void FindLaneDetectors(List<Detector> tmcDetectors, MovementTypes movementType,
            List<Detector> detectorsByDirection, LaneTypes laneType)
        {
            foreach (var detector in detectorsByDirection)
                if (detector.LaneTypeId == laneType)
                    if ((int)movementType == 1)
                    {
                        if (detector.MovementTypeId == MovementTypes.T ||
                            detector.MovementTypeId == MovementTypes.TR ||
                            detector.MovementTypeId == MovementTypes.TL)
                            tmcDetectors.Add(detector);
                    }
                    else if (detector.MovementType.Id == movementType)
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
