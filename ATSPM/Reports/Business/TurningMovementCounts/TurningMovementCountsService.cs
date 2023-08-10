using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.TurningMovementCounts
{
    public class TurningMovementCountsService
    {
        private readonly PlanService planService;
        public TurningMovementCountsService(PlanService planService )
        {
            this.planService = planService;
        }


        public TurningMovementCountsResult GetChartData(
            TurningMovementCountsOptions options,
            Approach approach,
            List<Detector> detectors,
            List<ControllerEventLog> events,
            List<ControllerEventLog> planEvents)
        {
            var plans = planService.GetBasicPlans(options.Start, options.End, approach.Signal.SignalId, planEvents);
            var movementTotals = new SortedDictionary<DateTime, int>();
            var laneTotals = new SortedDictionary<string, int>();
            var binSizeMultiplier = 60 / options.SelectedBinSize;
            var lanes = GetLaneVolumes(options, detectors, movementTotals, laneTotals, events);
            var totalVolumes = movementTotals.Select(m => new TotalVolume(m.Key, m.Value)).ToList();
            var totalVolume = movementTotals.Sum(m => m.Value);
            var binMultiplier = 60 / options.SelectedBinSize;
            var highLaneVolume = laneTotals.Any() ? laneTotals.Values.Max() : 0;
            var peakHourValue = GetPeakHour(movementTotals, binMultiplier);
            var laneUtilizationFactor = GetLaneUtilizationFactor(totalVolume, detectors.Count(), highLaneVolume);
            var peakHourMaxVolume = movementTotals.Max(m => m.Value);// GetPeakHourMaxVolume(options, MovementTotals, binMultiplier, peakHourValue.Key);
            var peakHourFactor = GetPeakHourFactor(peakHourValue.Value, peakHourMaxVolume, binMultiplier);
            

            return new TurningMovementCountsResult(
                "Turning Movement Counts",
                options.ApproachId,
                approach.Description,
                options.Start,
                options.End,
                approach.DirectionTypeId.ToString(),
                plans,
                lanes,
                totalVolumes,
                totalVolume / binMultiplier,
                peakHourValue.Key.ToShortTimeString() + " - " + peakHourValue.Key.AddHours(1).ToShortTimeString(),
                peakHourValue.Value / binMultiplier,
                peakHourFactor,
                laneUtilizationFactor
                );
        }

        private int GetPeakHourMaxVolume(
            TurningMovementCountsOptions options,
            SortedDictionary<DateTime, int> MovementTotals,
            int binMultiplier,
            DateTime peakHour)
        {
            var peakHourMaxVolume = 0;
            for (var i = 0; i < binMultiplier; i++)
                if (MovementTotals.ContainsKey(peakHour.AddMinutes(i * options.SelectedBinSize)))
                    if (peakHourMaxVolume < MovementTotals[peakHour.AddMinutes(i * options.SelectedBinSize)])
                        peakHourMaxVolume = MovementTotals[peakHour.AddMinutes(i * options.SelectedBinSize)];
            return peakHourMaxVolume;
        }

        private List<Lane> GetLaneVolumes(
            TurningMovementCountsOptions options,
            List<Detector> detectors,
            SortedDictionary<DateTime, int> MovementTotals,
            SortedDictionary<string, int> laneTotals,
            List<ControllerEventLog> events)
        {
            var laneVolumes = new List<Lane>();
            foreach (var detector in detectors.OrderBy(d => d.LaneNumber))
            {
                if (detectors != null && detectors.Any())// && detector.MovementTypeId != MovementTypes.TR && detector.MovementTypeId != MovementTypes.TL)
                {
                    var lane = new Lane();
                    var detectorEvents = events.Where(e => e.EventParam == detector.DetChannel).ToList();
                    var volumes = new VolumeCollection(options.Start, options.End, detectorEvents, options.SelectedBinSize);
                    lane.LaneNumber = detector.LaneNumber;
                    lane.LaneType = detector.LaneTypeId;
                    lane.MovementType = detector.MovementTypeId;
                    lane.Volume = new List<LaneVolume>();
                    foreach (var volume in volumes.Items)
                    {
                        lane.Volume.Add(new LaneVolume { StartTime = volume.StartTime, Volume = volume.DetectorCount });
                        //One of the calculations requires total volume by lane.  This if statment keeps a 
                        //running total of that volume and stores it in a dictonary with the lane number.
                        if (laneTotals.ContainsKey("L" + detector.LaneNumber))
                            laneTotals["L" + detector.LaneNumber] += volume.HourlyVolume;
                        else
                            laneTotals.Add("L" + detector.LaneNumber, volume.HourlyVolume);
                        //we need ot track the total number of cars (volume) for this movement.
                        //this uses a time/int dictionary.  The volume record for a given time is contibuted to by each lane.
                        //Then the movement total can be plotted on the graph
                        if (MovementTotals.ContainsKey(volume.StartTime))
                            MovementTotals[volume.StartTime] += volume.HourlyVolume;
                        else
                            MovementTotals.Add(volume.StartTime, volume.HourlyVolume);
                    }
                    laneVolumes.Add(lane);
                }
            }
            return laneVolumes;
        }

        public double? GetLaneUtilizationFactor(int totalVolume, int laneCount, int highLaneVolume)
        {
            if (laneCount > 0 && highLaneVolume > 0)
            {
                var fLU = Convert.ToDouble(totalVolume) /
                          (Convert.ToDouble(laneCount) * Convert.ToDouble(highLaneVolume));
                return SetSigFigs(fLU, 2);
            }
            else
            {
                return null;
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


        public KeyValuePair<DateTime, int> GetPeakHour(SortedDictionary<DateTime, int> dirVolumes, int binMultiplier)
        {
            var subTotal = 0;
            var peakHourValue = new KeyValuePair<DateTime, int>();

            var startTime = new DateTime();
            var iteratedVolumes = new SortedDictionary<DateTime, int>();

            for (var i = 0; i < dirVolumes.Count - (binMultiplier - 1); i++)
            {
                startTime = dirVolumes.ElementAt(i).Key;
                subTotal = 0;
                for (var x = 0; x < binMultiplier; x++)
                    subTotal = subTotal + dirVolumes.ElementAt(i + x).Value;
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
