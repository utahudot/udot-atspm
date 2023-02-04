using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Legacy.Common.Business;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.ApproachVolume
{
    [DataContract]
    public class ApproachVolume
    {
        private readonly ApproachVolumeOptions _approachVolumeOptions;
        private readonly IControllerEventLogRepository _controllerEventLogRepository;
        private readonly List<Approach> _primaryDirectionApproaches;
        private readonly List<Approach> _opposingDirectionApproaches;
        public VolumeCollection PrimaryDirectionVolume { get; private set; }
        public VolumeCollection OpposingDirectionVolume { get; private set; }
        public VolumeCollection CombinedDirectionsVolumes { get; private set; }
        public DirectionTypes PrimaryDirection { get; private set; }
        public DirectionTypes OpposingDirection { get; private set; }
        public DetectionType DetectionType { get; set; }
        public List<Data.Models.Detector> Detectors { get; set; } = new List<Data.Models.Detector>();
        public MetricInfo MetricInfo { get; set; } = new MetricInfo();
        public List<ControllerEventLog> PrimaryDetectorEvents { get; set; } = new List<ControllerEventLog>();
        public List<ControllerEventLog> OpposingDetectorEvents { get; set; } = new List<ControllerEventLog>();


        public ApproachVolume(
            List<Approach> primaryDirectionApproaches,
            List<Approach> opposingDirectionApproaches,
            ApproachVolumeOptions approachVolumeOptions,
            DirectionTypes opposingDirection,
            DetectionTypes detectionType,
            IDetectionTypeRepository detectionTypeRepository,
            IControllerEventLogRepository controllerEventLogRepository)
        {
            //var detectionTypeRepository = DetectionTypeRepositoryFactory.Create();
            DetectionType = detectionTypeRepository.Lookup(detectionType);
            PrimaryDirection = approachVolumeOptions.Direction;
            MetricInfo.Direction1 = PrimaryDirection.ToString();
            OpposingDirection = opposingDirection;
            _controllerEventLogRepository = controllerEventLogRepository;
            MetricInfo.Direction2 = opposingDirection.ToString();
            _approachVolumeOptions = approachVolumeOptions;
            _primaryDirectionApproaches = primaryDirectionApproaches;
            _opposingDirectionApproaches = opposingDirectionApproaches;
            SetVolume();
            SetVolumeMetrics();
        }

        public void SetVolume()
        {
            PrimaryDirectionVolume = SetVolumeByDetection(_primaryDirectionApproaches, PrimaryDetectorEvents);
            OpposingDirectionVolume = SetVolumeByDetection(_opposingDirectionApproaches, OpposingDetectorEvents);
        }

        private VolumeCollection SetVolumeByDetection(List<Approach> approaches, List<ControllerEventLog> detectorEvents)
        {

            foreach (var approach in approaches)
            {
                foreach (var detector in approach.Detectors)
                {
                    if (detector.DetectionTypes.Any(d => d.Id == DetectionType.Id))
                    {
                        if (detector.LaneType.Id == LaneTypes.V)
                        {
                            Detectors.Add(detector);
                            detectorEvents.AddRange(_controllerEventLogRepository.GetEventsByEventCodesParam(
                                detector.Approach.SignalId,
                                _approachVolumeOptions.StartDate,
                                _approachVolumeOptions.EndDate,
                                new List<int> { 82 },
                                detector.DetChannel,
                                detector.GetOffset(),
                                detector.LatencyCorrection));
                        }
                    }
                }
            }
            return new VolumeCollection(_approachVolumeOptions.StartDate, _approachVolumeOptions.EndDate, detectorEvents, _approachVolumeOptions.SelectedBinSize);
        }

        public void SetVolumeMetrics()
        {
            int binSizeMultiplier = 60 / _approachVolumeOptions.SelectedBinSize;
            SetCombinedVolumeStatistics(binSizeMultiplier);
            if (PrimaryDirectionVolume != null)
                SetPrimaryDirectionVolumeStatistics(binSizeMultiplier);
            if (OpposingDirectionVolume != null)
                SetOpposingDirectionVolumeStatistics(binSizeMultiplier);
        }

        private void SetOpposingDirectionVolumeStatistics(int binSizeMultiplier)
        {
            KeyValuePair<DateTime, int> direction2PeakHourItem = GetPeakHourVolumeItem(OpposingDirectionVolume);
            MetricInfo.Direction2PeakHourMaxValue = FindPeakValueinHour(direction2PeakHourItem.Key, OpposingDirectionVolume);
            MetricInfo.Direction2PeakHourFactor = GetPeakHourFactor(direction2PeakHourItem.Value, MetricInfo.Direction2PeakHourMaxValue, binSizeMultiplier);
            MetricInfo.Direction2PeakHourKFactor = GetPeakHourKFactor(direction2PeakHourItem);
            MetricInfo.Direction2PeakHourDFactor = GetPeakHourDFactor(direction2PeakHourItem.Key, direction2PeakHourItem.Value, PrimaryDirectionVolume, binSizeMultiplier);
            MetricInfo.Direction2PeakHourString = direction2PeakHourItem.Key.ToShortTimeString() + " - " + direction2PeakHourItem.Key.AddHours(1).ToShortTimeString();
            MetricInfo.Direction2PeakHourVolume = direction2PeakHourItem.Value;
            MetricInfo.Direction2Volume = OpposingDirectionVolume.Items.Sum(o => o.DetectorCount);
        }

        private void SetPrimaryDirectionVolumeStatistics(int binSizeMultiplier)
        {
            KeyValuePair<DateTime, int> direction1PeakHourItem = GetPeakHourVolumeItem(PrimaryDirectionVolume);
            MetricInfo.Direction1PeakHourMaxValue = FindPeakValueinHour(direction1PeakHourItem.Key, PrimaryDirectionVolume);
            MetricInfo.Direction1PeakHourFactor = GetPeakHourFactor(direction1PeakHourItem.Value, MetricInfo.Direction1PeakHourMaxValue, binSizeMultiplier);
            MetricInfo.Direction1PeakHourKFactor = GetPeakHourKFactor(direction1PeakHourItem);
            MetricInfo.Direction1PeakHourDFactor = GetPeakHourDFactor(direction1PeakHourItem.Key, direction1PeakHourItem.Value, OpposingDirectionVolume, binSizeMultiplier);
            MetricInfo.Direction1PeakHourString = direction1PeakHourItem.Key.ToShortTimeString() + " - " + direction1PeakHourItem.Key.AddHours(1).ToShortTimeString();
            MetricInfo.Direction1PeakHourVolume = direction1PeakHourItem.Value;
            MetricInfo.Direction1Volume = PrimaryDirectionVolume.Items.Sum(o => o.DetectorCount);
        }

        private void SetCombinedVolumeStatistics(int binSizeMultiplier)
        {
            CombinedDirectionsVolumes = new VolumeCollection(PrimaryDirectionVolume, OpposingDirectionVolume, _approachVolumeOptions.SelectedBinSize);
            KeyValuePair<DateTime, int> combinedPeakHourItem = GetPeakHourVolumeItem(CombinedDirectionsVolumes);
            MetricInfo.CombinedPeakHourValue = FindPeakValueinHour(combinedPeakHourItem.Key, CombinedDirectionsVolumes);
            MetricInfo.CombinedPeakHourFactor = GetPeakHourFactor(combinedPeakHourItem.Value, MetricInfo.CombinedPeakHourValue, binSizeMultiplier);
            MetricInfo.CombinedPeakHourKFactor = GetPeakHourKFactor(combinedPeakHourItem);
            MetricInfo.CombinedPeakHourString = combinedPeakHourItem.Key.ToShortTimeString() + " - " + combinedPeakHourItem.Key.AddHours(1).ToShortTimeString();
            MetricInfo.CombinedPeakHourVolume = combinedPeakHourItem.Value;
            MetricInfo.CombinedVolume = CombinedDirectionsVolumes.Items.Sum(c => c.DetectorCount);
        }

        private double GetPeakHourKFactor(KeyValuePair<DateTime, int> combinedPeakHourItem)
        {
            double combinedVolumeForPeakHour = CombinedDirectionsVolumes.Items
                .Where(v => v.StartTime >= combinedPeakHourItem.Key && v.StartTime < combinedPeakHourItem.Key.AddHours(1)).Sum(v => v.DetectorCount);
            double combinedVolume = CombinedDirectionsVolumes.Items.Sum(v => v.DetectorCount);
            return Math.Round(combinedVolumeForPeakHour / combinedVolume, 3);
        }

        public static double Round(double d, int digits)
        {
            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }

        private static double GetPeakHourFactor(int direction1PeakHourVolume, int direction1PeakHourMaxValue, int binSizeMultiplier)
        {
            double D1PHF = 0;
            if (direction1PeakHourMaxValue > 0)
            {
                D1PHF = Convert.ToDouble(direction1PeakHourVolume) / Convert.ToDouble(direction1PeakHourMaxValue * binSizeMultiplier);
                D1PHF = Round(D1PHF, 3);
            }

            return D1PHF;
        }

        protected KeyValuePair<DateTime, int> GetPeakHourVolumeItem(VolumeCollection volumes)
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

        protected int FindPeakValueinHour(DateTime startofHour, VolumeCollection volumeCollection)
        {
            int maxVolume = 0;
            if (volumeCollection.Items.Any(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1)))
            {
                maxVolume = volumeCollection.Items.Where(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1)).Max(v => v.DetectorCount);
            }
            return maxVolume;
        }

        protected double GetPeakHourDFactor(DateTime startofHour, int peakhourvolume, VolumeCollection volumes,
            int binMultiplier)
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
