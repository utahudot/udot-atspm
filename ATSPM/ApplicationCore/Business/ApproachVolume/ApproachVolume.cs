//using ATSPM.Application.Extensions;
//using ATSPM.Application.Reports.Business.Common;
//using ATSPM.Application.Repositories;
//using ATSPM.Data.Enums;
//using ATSPM.Data.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace ATSPM.Application.Reports.Business.ApproachVolume
//{
//    public class ApproachVolume
//    {
//        private readonly IIndianaEventLogRepository controllerEventLogRepository;
//        private readonly IDetectionTypeRepository detectionTypeRepository;

//        public ApproachVolume(
//            IIndianaEventLogRepository controllerEventLogRepository,
//            IDetectionTypeRepository detectionTypeRepository)
//        {
//            this.controllerEventLogRepository = controllerEventLogRepository;
//            this.detectionTypeRepository = detectionTypeRepository;
//        }

//        public ApproachVolumeData GetApproachVolumeData(
//            List<Approach> primaryDirectionApproaches,
//            List<Approach> opposingDirectionApproaches,
//            ApproachVolumeOptions options)
//        {
//            var metricInfo = new MetricInfo();
//            var primaryDirectionVolume = GetVolumeByDetection(primaryDirectionApproaches, options);
//            var opposingDirectionVolume = GetVolumeByDetection(opposingDirectionApproaches, options);
//            var combinedDirectionsVolumes = new VolumeCollection(primaryDirectionVolume, opposingDirectionVolume, options.SelectedBinSize);
//            SetVolumeMetrics(
//                options,
//                primaryDirectionVolume,
//                opposingDirectionVolume,
//                combinedDirectionsVolumes,
//                metricInfo);
//            return new ApproachVolumeData
//            {
//                CombinedDirectionsVolumes = combinedDirectionsVolumes,
//                MetricInfo = metricInfo,
//                PrimaryDirectionVolume = primaryDirectionVolume,
//                OpposingDirectionVolume = opposingDirectionVolume,
//                PrimaryDirection = primaryDirectionApproaches.First().DirectionTypeId,
//                OpposingDirection = opposingDirectionApproaches.First().DirectionTypeId
//            };
//        }

//        private VolumeCollection GetVolumeByDetection(
//            List<Approach> approaches,
//            ApproachVolumeOptions options)
//        {
//            var detectors = approaches.SelectMany(a => a.GetDetectorsForMetricType(7))
//                .Where(d => d.LaneTypeId == LaneTypes.V).ToList();

//            var detectorEvents = detectors.SelectMany(d => controllerEventLogRepository.GetEventsByEventCodesParam(
//                                d.Approach.LocationId,
//                                options.Start,
//                                options.End,
//                                new List<int> { 82 },
//                                d.DetectorChannel,
//                                d.GetOffset(),
//                                d.LatencyCorrection)).ToList();
//            return new VolumeCollection(
//                options.Start,
//                options.End,
//                detectorEvents,
//                options.SelectedBinSize);
//        }

//        public MetricInfo SetVolumeMetrics(
//            ApproachVolumeOptions options,
//            VolumeCollection primaryDirectionVolume,
//            VolumeCollection opposingDirectionVolume,
//            VolumeCollection combinedDirectionsVolumes,
//            MetricInfo metricInfo
//            )
//        {
//            int binSizeMultiplier = 60 / options.SelectedBinSize;

//            SetCombinedVolumeStatistics(
//                binSizeMultiplier,
//                primaryDirectionVolume,
//                opposingDirectionVolume,
//                combinedDirectionsVolumes,
//                metricInfo,
//                options.SelectedBinSize);
//            if (primaryDirectionVolume != null)
//                SetPrimaryDirectionVolumeStatistics(
//                    binSizeMultiplier,
//                    primaryDirectionVolume,
//                    opposingDirectionVolume,
//                    combinedDirectionsVolumes,
//                    metricInfo);
//            if (opposingDirectionVolume != null)
//                SetOpposingDirectionVolumeStatistics(
//                    binSizeMultiplier,
//                    primaryDirectionVolume,
//                    opposingDirectionVolume,
//                    combinedDirectionsVolumes,
//                    metricInfo);
//            return metricInfo;
//        }

//        private void SetOpposingDirectionVolumeStatistics(int binSizeMultiplier,
//            VolumeCollection primaryDirectionVolume,
//            VolumeCollection opposingDirectionVolume,
//            VolumeCollection combinedDirectionsVolume,
//            MetricInfo metricInfo)
//        {
//            KeyValuePair<DateTime, int> direction2PeakHourItem = GetPeakHourVolumeItem(opposingDirectionVolume);
//            metricInfo.Direction2PeakHourMaxValue = FindPeakValueinHour(direction2PeakHourItem.Key, opposingDirectionVolume);
//            metricInfo.Direction2PeakHourFactor = GetPeakHourFactor(direction2PeakHourItem.Value, metricInfo.Direction2PeakHourMaxValue, binSizeMultiplier);
//            metricInfo.Direction2PeakHourKFactor = GetPeakHourKFactor(direction2PeakHourItem, combinedDirectionsVolume);
//            metricInfo.Direction2PeakHourDFactor = GetPeakHourDFactor(direction2PeakHourItem.Key, direction2PeakHourItem.Value, primaryDirectionVolume, binSizeMultiplier);
//            metricInfo.Direction2PeakHourString = direction2PeakHourItem.Key.ToShortTimeString() + " - " + direction2PeakHourItem.Key.AddHours(1).ToShortTimeString();
//            metricInfo.Direction2PeakHourVolume = direction2PeakHourItem.Value;
//            metricInfo.Direction2Volume = opposingDirectionVolume.Cycles.Sum(o => o.DetectorCount);
//        }

//        private void SetPrimaryDirectionVolumeStatistics(
//            int binSizeMultiplier,
//            VolumeCollection primaryDirectionVolume,
//            VolumeCollection opposingDirectionVolume,
//            VolumeCollection combinedDirectionsVolume,
//            MetricInfo metricInfo)
//        {
//            KeyValuePair<DateTime, int> direction1PeakHourItem = GetPeakHourVolumeItem(primaryDirectionVolume);
//            metricInfo.Direction1PeakHourMaxValue = FindPeakValueinHour(direction1PeakHourItem.Key, primaryDirectionVolume);
//            metricInfo.Direction1PeakHourFactor = GetPeakHourFactor(direction1PeakHourItem.Value, metricInfo.Direction1PeakHourMaxValue, binSizeMultiplier);
//            metricInfo.Direction1PeakHourKFactor = GetPeakHourKFactor(direction1PeakHourItem, combinedDirectionsVolume);
//            metricInfo.Direction1PeakHourDFactor = GetPeakHourDFactor(direction1PeakHourItem.Key, direction1PeakHourItem.Value, opposingDirectionVolume, binSizeMultiplier);
//            metricInfo.Direction1PeakHourString = direction1PeakHourItem.Key.ToShortTimeString() + " - " + direction1PeakHourItem.Key.AddHours(1).ToShortTimeString();
//            metricInfo.Direction1PeakHourVolume = direction1PeakHourItem.Value;
//            metricInfo.Direction1Volume = primaryDirectionVolume.Cycles.Sum(o => o.DetectorCount);
//        }

//        private void SetCombinedVolumeStatistics(int binSizeMultiplier,
//            VolumeCollection primaryDirectionVolume,
//            VolumeCollection opposingDirectionVolume,
//            VolumeCollection combinedDirectionsVolume,
//            MetricInfo metricInfo,
//            int selectedBinSize)
//        {
//            KeyValuePair<DateTime, int> combinedPeakHourItem = GetPeakHourVolumeItem(combinedDirectionsVolume);
//            metricInfo.CombinedPeakHourValue = FindPeakValueinHour(combinedPeakHourItem.Key, combinedDirectionsVolume);
//            metricInfo.CombinedPeakHourFactor = GetPeakHourFactor(combinedPeakHourItem.Value, metricInfo.CombinedPeakHourValue, binSizeMultiplier);
//            metricInfo.CombinedPeakHourKFactor = GetPeakHourKFactor(combinedPeakHourItem, combinedDirectionsVolume);
//            metricInfo.CombinedPeakHourString = combinedPeakHourItem.Key.ToShortTimeString() + " - " + combinedPeakHourItem.Key.AddHours(1).ToShortTimeString();
//            metricInfo.CombinedPeakHourVolume = combinedPeakHourItem.Value;
//            metricInfo.CombinedVolume = combinedDirectionsVolume.Cycles.Sum(c => c.DetectorCount);
//        }

//        private double GetPeakHourKFactor(
//            KeyValuePair<DateTime, int> combinedPeakHourItem,
//            VolumeCollection combindedDirectionsVolumes)
//        {
//            double combinedVolumeForPeakHour = combindedDirectionsVolumes.Cycles
//                .Where(v => v.StartTime >= combinedPeakHourItem.Key && v.StartTime < combinedPeakHourItem.Key.AddHours(1)).Sum(v => v.DetectorCount);
//            double combinedVolume = combindedDirectionsVolumes.Cycles.Sum(v => v.DetectorCount);
//            return Math.Round(combinedVolumeForPeakHour / combinedVolume, 3);
//        }

//        public static double Round(double d, int digits)
//        {
//            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
//            return scale * Math.Round(d / scale, digits);
//        }

//        private static double GetPeakHourFactor(int direction1PeakHourVolume, int direction1PeakHourMaxValue, int binSizeMultiplier)
//        {
//            double D1PHF = 0;
//            if (direction1PeakHourMaxValue > 0)
//            {
//                D1PHF = Convert.ToDouble(direction1PeakHourVolume) / Convert.ToDouble(direction1PeakHourMaxValue * binSizeMultiplier);
//                D1PHF = Round(D1PHF, 3);
//            }

//            return D1PHF;
//        }

//        protected KeyValuePair<DateTime, int> GetPeakHourVolumeItem(VolumeCollection volumes)
//        {
//            KeyValuePair<DateTime, int> peakHourValue = new KeyValuePair<DateTime, int>();
//            SortedDictionary<DateTime, int> iteratedVolumes = new SortedDictionary<DateTime, int>();
//            foreach (var volume in volumes.Cycles)
//            {
//                iteratedVolumes.Add(volume.StartTime, volumes.Cycles.Where(v => v.StartTime >= volume.StartTime && v.StartTime < volume.StartTime.AddHours(1)).Sum(v => v.DetectorCount));
//            }
//            peakHourValue = iteratedVolumes.OrderByDescending(i => i.Value).FirstOrDefault();
//            return peakHourValue;
//        }

//        protected int FindPeakValueinHour(DateTime startofHour, VolumeCollection volumeCollection)
//        {
//            int maxVolume = 0;
//            if (volumeCollection.Cycles.Any(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1)))
//            {
//                maxVolume = volumeCollection.Cycles.Where(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1)).Max(v => v.DetectorCount);
//            }
//            return maxVolume;
//        }

//        protected double GetPeakHourDFactor(DateTime startofHour, int peakhourvolume, VolumeCollection volumes,
//            int binMultiplier)
//        {
//            int totalVolume = 0;
//            double PHDF = 0;
//            if (volumes.Cycles.Any(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1)))
//            {
//                totalVolume = volumes.Cycles
//                    .Where(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1))
//                    .Sum(v => v.DetectorCount);
//            }
//            //totalVolume /= binMultiplier;
//            totalVolume += peakhourvolume;
//            if (totalVolume > 0)
//                PHDF = Round(Convert.ToDouble(peakhourvolume) / Convert.ToDouble(totalVolume), 3);
//            else
//                PHDF = 0;
//            return PHDF;
//        }
//    }

//    public class ApproachVolumeData
//    {
//        public VolumeCollection PrimaryDirectionVolume { get; set; }
//        public VolumeCollection OpposingDirectionVolume { get; set; }
//        public VolumeCollection CombinedDirectionsVolumes { get; set; }
//        public DirectionTypes PrimaryDirection { get; set; }
//        public DirectionTypes OpposingDirection { get; set; }
//        public DetectionType DetectionType { get; set; }
//        public List<Data.Models.Detector> Detectors { get; set; } = new List<Data.Models.Detector>();
//        public MetricInfo MetricInfo { get; set; } = new MetricInfo();
//        public List<IndianaEvent> PrimaryDetectorEvents { get; set; } = new List<IndianaEvent>();
//        public List<IndianaEvent> OpposingDetectorEvents { get; set; } = new List<IndianaEvent>();
//    }
//}

////Here is my c# class using ATSPM.Application.Extensions;
////using ATSPM.Application.Reports.Business.Common;
////using ATSPM.Application.Repositories;
////using ATSPM.Data.Enums;
////using ATSPM.Data.Models;
////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.Runtime.Serialization;

////namespace ATSPM.Application.Reports.Business.ApproachVolume
////{
////    [DataContract]
////    public class ApproachVolume
////    {
////        private readonly ApproachVolumeOptions _approachVolumeOptions;
////        private readonly IIndianaEventLogRepository _controllerEventLogRepository;
////        private readonly List<Approach> _primaryDirectionApproaches;
////        private readonly List<Approach> _opposingDirectionApproaches;
////        public VolumeCollection PrimaryDirectionVolume { get; private set; }
////        public VolumeCollection OpposingDirectionVolume { get; private set; }
////        public VolumeCollection CombinedDirectionsVolumes { get; private set; }
////        public DirectionTypes PrimaryDirection { get; private set; }
////        public DirectionTypes OpposingDirection { get; private set; }
////        public DetectionType DetectionType { get; set; }
////        public List<Data.Models.Detector> Detectors { get; set; } = new List<Data.Models.Detector>();
////        public MetricInfo MetricInfo { get; set; } = new MetricInfo();
////        public List<IndianaEvent> PrimaryDetectorEvents { get; set; } = new List<IndianaEvent>();
////        public List<IndianaEvent> OpposingDetectorEvents { get; set; } = new List<IndianaEvent>();


////        public ApproachVolume(
////            List<Approach> primaryDirectionApproaches,
////            List<Approach> opposingDirectionApproaches,
////            ApproachVolumeOptions approachVolumeOptions,
////            DirectionTypes opposingDirection,
////            DetectionTypes detectionType,
////            IDetectionTypeRepository detectionTypeRepository,
////            IIndianaEventLogRepository controllerEventLogRepository)
////        {
////            //var detectionTypeRepository = DetectionTypeRepositoryFactory.Create();
////            DetectionType = detectionTypeRepository.Lookup(detectionType);
////            PrimaryDirection = approachVolumeOptions.Direction;
////            MetricInfo.Direction1 = PrimaryDirection.ToString();
////            OpposingDirection = opposingDirection;
////            _controllerEventLogRepository = controllerEventLogRepository;
////            MetricInfo.Direction2 = opposingDirection.ToString();
////            _approachVolumeOptions = approachVolumeOptions;
////            _primaryDirectionApproaches = primaryDirectionApproaches;
////            _opposingDirectionApproaches = opposingDirectionApproaches;
////            SetVolume();
////            SetVolumeMetrics();
////        }

////        public void SetVolume()
////        {
////            PrimaryDirectionVolume = SetVolumeByDetection(_primaryDirectionApproaches, PrimaryDetectorEvents);
////            OpposingDirectionVolume = SetVolumeByDetection(_opposingDirectionApproaches, OpposingDetectorEvents);
////        }

////        private VolumeCollection SetVolumeByDetection(List<Approach> approaches, List<IndianaEvent> detectorEvents)
////        {

////            foreach (var approach in approaches)
////            {
////                foreach (var detector in approach.Detectors)
////                {
////                    if (detector.DetectionTypes.Any(d => d.Id == DetectionType.Id))
////                    {
////                        if (detector.LaneType.Id == LaneTypes.V)
////                        {
////                            Detectors.Add(detector);
////                            detectorEvents.AddRange(_controllerEventLogRepository.GetEventsByEventCodesParam(
////                                detector.Approach.LocationId,
////                                _approachVolumeOptions.Start,
////                                _approachVolumeOptions.End,
////                                new List<int> { 82 },
////                                detector.DetectorChannel,
////                                detector.GetOffset(),
////                                detector.LatencyCorrection));
////                        }
////                    }
////                }
////            }
////            return new VolumeCollection(_approachVolumeOptions.Start, _approachVolumeOptions.End, detectorEvents, _approachVolumeOptions.SelectedBinSize);
////        }

////        public void SetVolumeMetrics()
////        {
////            int binSizeMultiplier = 60 / _approachVolumeOptions.SelectedBinSize;
////            SetCombinedVolumeStatistics(binSizeMultiplier);
////            if (PrimaryDirectionVolume != null)
////                SetPrimaryDirectionVolumeStatistics(binSizeMultiplier);
////            if (OpposingDirectionVolume != null)
////                SetOpposingDirectionVolumeStatistics(binSizeMultiplier);
////        }

////        private void SetOpposingDirectionVolumeStatistics(int binSizeMultiplier)
////        {
////            KeyValuePair<DateTime, int> direction2PeakHourItem = GetPeakHourVolumeItem(OpposingDirectionVolume);
////            MetricInfo.Direction2PeakHourMaxValue = FindPeakValueinHour(direction2PeakHourItem.Key, OpposingDirectionVolume);
////            MetricInfo.Direction2PeakHourFactor = GetPeakHourFactor(direction2PeakHourItem.Value, MetricInfo.Direction2PeakHourMaxValue, binSizeMultiplier);
////            MetricInfo.Direction2PeakHourKFactor = GetPeakHourKFactor(direction2PeakHourItem);
////            MetricInfo.Direction2PeakHourDFactor = GetPeakHourDFactor(direction2PeakHourItem.Key, direction2PeakHourItem.Value, PrimaryDirectionVolume, binSizeMultiplier);
////            MetricInfo.Direction2PeakHourString = direction2PeakHourItem.Key.ToShortTimeString() + " - " + direction2PeakHourItem.Key.AddHours(1).ToShortTimeString();
////            MetricInfo.Direction2PeakHourVolume = direction2PeakHourItem.Value;
////            MetricInfo.Direction2Volume = OpposingDirectionVolume.Cycles.Sum(o => o.DetectorCount);
////        }

////        private void SetPrimaryDirectionVolumeStatistics(int binSizeMultiplier)
////        {
////            KeyValuePair<DateTime, int> direction1PeakHourItem = GetPeakHourVolumeItem(PrimaryDirectionVolume);
////            MetricInfo.Direction1PeakHourMaxValue = FindPeakValueinHour(direction1PeakHourItem.Key, PrimaryDirectionVolume);
////            MetricInfo.Direction1PeakHourFactor = GetPeakHourFactor(direction1PeakHourItem.Value, MetricInfo.Direction1PeakHourMaxValue, binSizeMultiplier);
////            MetricInfo.Direction1PeakHourKFactor = GetPeakHourKFactor(direction1PeakHourItem);
////            MetricInfo.Direction1PeakHourDFactor = GetPeakHourDFactor(direction1PeakHourItem.Key, direction1PeakHourItem.Value, OpposingDirectionVolume, binSizeMultiplier);
////            MetricInfo.Direction1PeakHourString = direction1PeakHourItem.Key.ToShortTimeString() + " - " + direction1PeakHourItem.Key.AddHours(1).ToShortTimeString();
////            MetricInfo.Direction1PeakHourVolume = direction1PeakHourItem.Value;
////            MetricInfo.Direction1Volume = PrimaryDirectionVolume.Cycles.Sum(o => o.DetectorCount);
////        }

////        private void SetCombinedVolumeStatistics(int binSizeMultiplier)
////        {
////            CombinedDirectionsVolumes = new VolumeCollection(PrimaryDirectionVolume, OpposingDirectionVolume, _approachVolumeOptions.SelectedBinSize);
////            KeyValuePair<DateTime, int> combinedPeakHourItem = GetPeakHourVolumeItem(CombinedDirectionsVolumes);
////            MetricInfo.CombinedPeakHourValue = FindPeakValueinHour(combinedPeakHourItem.Key, CombinedDirectionsVolumes);
////            MetricInfo.CombinedPeakHourFactor = GetPeakHourFactor(combinedPeakHourItem.Value, MetricInfo.CombinedPeakHourValue, binSizeMultiplier);
////            MetricInfo.CombinedPeakHourKFactor = GetPeakHourKFactor(combinedPeakHourItem);
////            MetricInfo.CombinedPeakHourString = combinedPeakHourItem.Key.ToShortTimeString() + " - " + combinedPeakHourItem.Key.AddHours(1).ToShortTimeString();
////            MetricInfo.CombinedPeakHourVolume = combinedPeakHourItem.Value;
////            MetricInfo.CombinedVolume = CombinedDirectionsVolumes.Cycles.Sum(c => c.DetectorCount);
////        }

////        private double GetPeakHourKFactor(KeyValuePair<DateTime, int> combinedPeakHourItem)
////        {
////            double combinedVolumeForPeakHour = CombinedDirectionsVolumes.Cycles
////                .Where(v => v.StartTime >= combinedPeakHourItem.Key && v.StartTime < combinedPeakHourItem.Key.AddHours(1)).Sum(v => v.DetectorCount);
////            double combinedVolume = CombinedDirectionsVolumes.Cycles.Sum(v => v.DetectorCount);
////            return Math.Round(combinedVolumeForPeakHour / combinedVolume, 3);
////        }

////        public static double Round(double d, int digits)
////        {
////            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
////            return scale * Math.Round(d / scale, digits);
////        }

////        private static double GetPeakHourFactor(int direction1PeakHourVolume, int direction1PeakHourMaxValue, int binSizeMultiplier)
////        {
////            double D1PHF = 0;
////            if (direction1PeakHourMaxValue > 0)
////            {
////                D1PHF = Convert.ToDouble(direction1PeakHourVolume) / Convert.ToDouble(direction1PeakHourMaxValue * binSizeMultiplier);
////                D1PHF = Round(D1PHF, 3);
////            }

////            return D1PHF;
////        }

////        protected KeyValuePair<DateTime, int> GetPeakHourVolumeItem(VolumeCollection volumes)
////        {
////            KeyValuePair<DateTime, int> peakHourValue = new KeyValuePair<DateTime, int>();
////            SortedDictionary<DateTime, int> iteratedVolumes = new SortedDictionary<DateTime, int>();
////            foreach (var volume in volumes.Cycles)
////            {
////                iteratedVolumes.Add(volume.StartTime, volumes.Cycles.Where(v => v.StartTime >= volume.StartTime && v.StartTime < volume.StartTime.AddHours(1)).Sum(v => v.DetectorCount));
////            }
////            peakHourValue = iteratedVolumes.OrderByDescending(i => i.Value).FirstOrDefault();
////            return peakHourValue;
////        }

////        protected int FindPeakValueinHour(DateTime startofHour, VolumeCollection volumeCollection)
////        {
////            int maxVolume = 0;
////            if (volumeCollection.Cycles.Any(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1)))
////            {
////                maxVolume = volumeCollection.Cycles.Where(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1)).Max(v => v.DetectorCount);
////            }
////            return maxVolume;
////        }

////        protected double GetPeakHourDFactor(DateTime startofHour, int peakhourvolume, VolumeCollection volumes,
////            int binMultiplier)
////        {
////            int totalVolume = 0;
////            double PHDF = 0;
////            if (volumes.Cycles.Any(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1)))
////            {
////                totalVolume = volumes.Cycles
////                    .Where(v => v.StartTime >= startofHour && v.StartTime < startofHour.AddHours(1))
////                    .Sum(v => v.DetectorCount);
////            }
////            //totalVolume /= binMultiplier;
////            totalVolume += peakhourvolume;
////            if (totalVolume > 0)
////                PHDF = Round(Convert.ToDouble(peakhourvolume) / Convert.ToDouble(totalVolume), 3);
////            else
////                PHDF = 0;
////            return PHDF;
////        }
////    }
////}
