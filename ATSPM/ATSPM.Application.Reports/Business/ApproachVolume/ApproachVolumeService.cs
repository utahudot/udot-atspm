using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.ViewModels.ApproachVolume;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using Legacy.Common.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ATSPM.Application.Reports.Business.ApproachVolume
{
    public class ApproachVolumeService
    {
        private readonly ISignalRepository signalRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly IDetectionTypeRepository detectionTypeRepository;
        public ApproachVolumeOptions Options { get; private set; }

        public ApproachVolumeService(
            ISignalRepository signalRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            IDetectionTypeRepository detectionTypeRepository)
        {
            this.signalRepository = signalRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.detectionTypeRepository = detectionTypeRepository;
        }

        public static double GetPeakHourKFactor(double d, int digits)

        {
            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }

        public ApproachVolumeResult GetChartData(
            ApproachVolumeOptions options)
        {
            DirectionTypes opposingDirection = GetOpposingDirection(options);
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalId);
            var primaryApproaches = signal.Approaches.Where(a => a.DirectionTypeId == options.Direction).ToList();
            var opposingApproaches = signal.Approaches.Where(a => a.DirectionTypeId == opposingDirection).ToList();
            ApproachVolume approachVolume = new ApproachVolume(
                primaryApproaches,
                opposingApproaches,
                options,
                opposingDirection,
                options.DetectionType,
                detectionTypeRepository,
                controllerEventLogRepository);
            var direction1Volumes = AddPrimaryDirectionSeries(approachVolume);
            var direction2Volumes = AddOpposingDirectionSeries(approachVolume);

            var primaryDirectionTotalVolume = Convert.ToDouble(approachVolume.PrimaryDirectionVolume.Items.Sum(d => d.HourlyVolume));
            var opposingDirectionTotalVolume = Convert.ToDouble(approachVolume.OpposingDirectionVolume.Items.Sum(d => d.HourlyVolume));

            DateTime startTime, endTime;
            SetStartTimeAndEndTime(approachVolume.PrimaryDirectionVolume, approachVolume.OpposingDirectionVolume, out startTime, out endTime);
            int binSizeMultiplier = 60 / options.SelectedBinSize;
            SortedDictionary<DateTime, int> combinedDirectionVolumes = new SortedDictionary<DateTime, int>();
            CombineDirectionVolumes(direction1Volumes, direction2Volumes, combinedDirectionVolumes);
            KeyValuePair<DateTime, int> combinedPeakHourItem = GetPeakHourVolumeItem(approachVolume.CombinedDirectionsVolumes, binSizeMultiplier);
            int combinedPeakHourValue = FindPeakValueinHour(combinedPeakHourItem.Key, approachVolume.CombinedDirectionsVolumes, binSizeMultiplier);
            double combinedPeakHourFactor = GetPeakHourFactor(combinedPeakHourItem.Value, combinedPeakHourValue, binSizeMultiplier);
            double combinedPeakHourKFactor = GetPeakHourKFactor(Convert.ToDouble(combinedPeakHourItem.Value) / primaryDirectionTotalVolume, 3);
            string combinedPeakHourString = combinedPeakHourItem.Key.ToShortTimeString() + " - " + combinedPeakHourItem.Key.AddHours(1).ToShortTimeString();
            int combinedVolume = combinedDirectionVolumes.Sum(c => c.Value) / binSizeMultiplier;
            KeyValuePair<DateTime, int> primayDirectionPeakHourItem = GetPeakHourVolumeItem(approachVolume.PrimaryDirectionVolume, binSizeMultiplier);
            int primaryDirectionPeakHourValue = FindPeakValueinHour(primayDirectionPeakHourItem.Key, approachVolume.PrimaryDirectionVolume, binSizeMultiplier);
            double primaryDirectionPeakHourFactor = GetPeakHourFactor(primayDirectionPeakHourItem.Value, primaryDirectionPeakHourValue, binSizeMultiplier);
            double primaryDirectionPeakHourKFactor = GetPeakHourKFactor(Convert.ToDouble(primayDirectionPeakHourItem.Value) / Convert.ToDouble(primaryDirectionTotalVolume), 3);
            double primaryDirectionPeakHourDFactor = GetPeakHourDFactor(primayDirectionPeakHourItem.Key, primayDirectionPeakHourItem.Value, approachVolume.OpposingDirectionVolume, binSizeMultiplier);
            string primaryDirectionPeakHourString = primayDirectionPeakHourItem.Key.ToShortTimeString() + " - " + primayDirectionPeakHourItem.Key.AddHours(1).ToShortTimeString();
            KeyValuePair<DateTime, int> opposingDirectionPeakHourItem = GetPeakHourVolumeItem(approachVolume.OpposingDirectionVolume, binSizeMultiplier);
            int opposingDirectionPeakValueInHour = FindPeakValueinHour(opposingDirectionPeakHourItem.Key, approachVolume.OpposingDirectionVolume, binSizeMultiplier);
            double opposingDirectionPeakHourFactor = GetPeakHourFactor(opposingDirectionPeakHourItem.Value, opposingDirectionPeakValueInHour, binSizeMultiplier);
            double opposingDirectionPeakHourKFactor = GetPeakHourKFactor(Convert.ToDouble(opposingDirectionPeakHourItem.Value) / Convert.ToDouble(opposingDirectionTotalVolume), 3);
            double opposingDirectionPeakHourDFactor = GetPeakHourDFactor(opposingDirectionPeakHourItem.Key, opposingDirectionPeakHourItem.Value, approachVolume.PrimaryDirectionVolume, binSizeMultiplier);
            string opposingDirectionPeakHourString = opposingDirectionPeakHourItem.Key.ToShortTimeString() + " - " + opposingDirectionPeakHourItem.Key.AddHours(1).ToShortTimeString();
            var detector = primaryApproaches.First().GetAllDetectorsOfDetectionType(options.DetectionType).First();
            var distanceFromStopBar = detector.DistanceFromStopBar.HasValue ? detector.DistanceFromStopBar.Value : 0;

            return new ApproachVolumeResult(
                "Approach Volume",
                options.SignalId,
                signal.PrimaryName + "@" + signal.SecondaryName,
                options.StartDate,
                options.EndDate,
                options.DetectionType,
                distanceFromStopBar,
                approachVolume.PrimaryDirection.ToString(),
                AddPrimaryDirectionSeries(approachVolume),
                approachVolume.OpposingDirection.ToString(),
                AddOpposingDirectionSeries(approachVolume),
                AddCombindedDirectionSeries(approachVolume),
                GetDFactorSeries(approachVolume.OpposingDirectionVolume, approachVolume.CombinedDirectionsVolumes),
                GetDFactorSeries(approachVolume.OpposingDirectionVolume, approachVolume.CombinedDirectionsVolumes),
                combinedPeakHourString,
                combinedPeakHourKFactor,
                combinedPeakHourValue,
                combinedPeakHourFactor,
                combinedVolume,
                primaryDirectionPeakHourString,
                primaryDirectionPeakHourKFactor,
                primayDirectionPeakHourItem.Value,
                primaryDirectionPeakHourFactor,
                Convert.ToInt32(primaryDirectionTotalVolume),
                opposingDirectionPeakHourString,
                opposingDirectionPeakHourKFactor,
                opposingDirectionPeakHourItem.Value,
                opposingDirectionPeakHourFactor,
                Convert.ToInt32(opposingDirectionTotalVolume)
                );
        }

        private static DirectionTypes GetOpposingDirection(ApproachVolumeOptions options)
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
                result.Add(new DFactors(approachVolume.Items[i].StartTime, Convert.ToDouble(approachVolume.Items[i].HourlyVolume) / Convert.ToDouble(combinedVolume.Items[i].HourlyVolume)));
            }
            return result;
        }

        private List<DirectionVolumes> AddCombindedDirectionSeries(ApproachVolume approachVolume)
        {
            if (approachVolume.CombinedDirectionsVolumes.Items.Count > 0 && Options.ShowTotalVolume)
            {
                return approachVolume.CombinedDirectionsVolumes.Items.ConvertAll(x => new DirectionVolumes(x.StartTime, x.HourlyVolume));
            }
            return new List<DirectionVolumes>();
        }

        private List<DirectionVolumes> AddOpposingDirectionSeries(ApproachVolume approachVolume)
        {
            if (approachVolume.OpposingDirectionVolume.Items.Count > 0)
            {
                if (Options.ShowNbEbVolume && (approachVolume.OpposingDirection == DirectionTypes.NB || approachVolume.OpposingDirection == DirectionTypes.EB) ||
                    Options.ShowSbWbVolume && (approachVolume.OpposingDirection == DirectionTypes.SB || approachVolume.OpposingDirection == DirectionTypes.WB))
                {
                    return approachVolume.OpposingDirectionVolume.Items.ConvertAll(x => new DirectionVolumes(x.StartTime, x.HourlyVolume));
                }
            }
            return new List<DirectionVolumes>();
        }

        private List<DirectionVolumes> AddPrimaryDirectionSeries(ApproachVolume approachVolume)
        {
            if (approachVolume.PrimaryDirectionVolume.Items.Count > 0)
            {
                if (Options.ShowNbEbVolume && (approachVolume.PrimaryDirection == DirectionTypes.NB || approachVolume.PrimaryDirection == DirectionTypes.EB) ||
                    Options.ShowSbWbVolume && (approachVolume.PrimaryDirection == DirectionTypes.SB || approachVolume.PrimaryDirection == DirectionTypes.WB))
                {
                    return approachVolume.PrimaryDirectionVolume.Items.ConvertAll(x => new DirectionVolumes(x.StartTime, x.HourlyVolume));
                }
            }
            return new List<DirectionVolumes>();
        }

        private static DataTable CreateAndSetVolumeMetricsTable(string direction1, string direction2, int direction1TotalVolume, int direction2TotalVolume,
            DateTime startTime, DateTime endTime, int combinedPeakVolume, double combinedPeakHourFactor, string combinedPeakHourString,
            int direction1PeakHourVolume, double direction1PeakHourFactor, int direction2PeakHourVolume, double direction2PeakHourFactor,
            string direction1PeakHourString, string direction2PeakHourString, int totalVolume, double peakHourKFactor, double direction1PeakHourKFactor,
            double direction1PeakHourDFactor, double direction2PeakHourKFactor, double direction2PeakHourDFactor)
        {
            DataTable volumeMetricsTable = new DataTable();
            DataColumn descriptionColumn = new DataColumn();
            DataColumn valueColumn = new DataColumn();
            descriptionColumn.ColumnName = "Metric";
            valueColumn.ColumnName = "Values";
            volumeMetricsTable.Columns.Add(descriptionColumn);
            volumeMetricsTable.Columns.Add(valueColumn);


            volumeMetricsTable.Rows.Add("Total Volume", totalVolume.ToString("N0"));
            volumeMetricsTable.Rows.Add("Peak Hour", combinedPeakHourString);
            volumeMetricsTable.Rows.Add("Peak Hour Volume", string.Format("{0:#,0}", combinedPeakVolume));
            volumeMetricsTable.Rows.Add("PHF", combinedPeakHourFactor.ToString());

            if (IsValidTimePeriodForKFactors(startTime, endTime))
            {
                volumeMetricsTable.Rows.Add("Peak-Hour K-factor", peakHourKFactor);
                volumeMetricsTable.Rows.Add(direction1 + " Peak-Hour K-factor", direction1PeakHourKFactor);
                volumeMetricsTable.Rows.Add(direction2 + " Peak-Hour K-factor", direction2PeakHourKFactor);
            }
            else
            {
                volumeMetricsTable.Rows.Add("Peak-Hour K-factor", "NA");
                volumeMetricsTable.Rows.Add(direction1 + " Peak-Hour K-factor", "NA");
                volumeMetricsTable.Rows.Add(direction2 + " Peak-Hour K-factor", "NA");
            }

            volumeMetricsTable.Rows.Add("", "");
            volumeMetricsTable.Rows.Add(direction1 + " Total Volume", direction1TotalVolume.ToString("N0"));
            volumeMetricsTable.Rows.Add(direction1 + " Peak Hour", direction1PeakHourString);
            volumeMetricsTable.Rows.Add(direction1 + " Peak Hour Volume", string.Format("{0:#,0}", direction1PeakHourVolume));
            volumeMetricsTable.Rows.Add(direction1 + " PHF", direction1PeakHourFactor.ToString());


            volumeMetricsTable.Rows.Add(direction1 + " Peak-Hour D-factor", direction1PeakHourDFactor);
            volumeMetricsTable.Rows.Add("", "");
            volumeMetricsTable.Rows.Add(direction2 + " Total Volume", direction2TotalVolume.ToString("N0"));
            volumeMetricsTable.Rows.Add(direction2 + " Peak Hour", direction2PeakHourString);
            volumeMetricsTable.Rows.Add(direction2 + " Peak Hour Volume", string.Format("{0:#,0}", direction2PeakHourVolume));
            volumeMetricsTable.Rows.Add(direction2 + " PHF", direction2PeakHourFactor.ToString("N0"));


            volumeMetricsTable.Rows.Add(direction2 + " Peak-Hour D-factor", direction2PeakHourDFactor);
            return volumeMetricsTable;
        }

        private static double GetPeakHourFactor(int direction1PeakHourVolume, int D1PHvol, int binSizeMultiplier)
        {
            double D1PHF = 0;
            if (D1PHvol > 0)
            {
                D1PHF = Convert.ToDouble(direction1PeakHourVolume) / Convert.ToDouble(D1PHvol);
                D1PHF = GetPeakHourKFactor(D1PHF, 3);
            }

            return D1PHF / binSizeMultiplier;
        }

        private static void CombineDirectionVolumes(List<DirectionVolumes> direction1Volumes, List<DirectionVolumes> direction2Volumes, SortedDictionary<DateTime, int> combinedDirectionVolumes)
        {
            foreach (DirectionVolumes current in direction1Volumes)
            {
                var index = direction2Volumes.FindIndex(d => d.StartTime == current.StartTime);
                if (index >= 0)
                    combinedDirectionVolumes.Add(current.StartTime, direction2Volumes[index].Volume + current.Volume);
            }
        }

        private static bool IsValidTimePeriodForKFactors(DateTime startTime, DateTime endTime)
        {
            TimeSpan timeDiff = endTime.Subtract(startTime);
            bool validKfactors = timeDiff.TotalHours >= 23 && timeDiff.TotalHours < 25;
            return validKfactors;
        }

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
                iteratedVolumes.Add(volume.StartTime, volumes.Items.Where(v => v.StartTime >= volume.StartTime && v.StartTime < volume.StartTime.AddHours(1)).Sum(v => v.HourlyVolume));
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
                    if (maxVolume < volDic.Items.FirstOrDefault(i => i.StartTime == StartofHour).HourlyVolume)
                        maxVolume = volDic.Items.FirstOrDefault(i => i.StartTime == StartofHour).HourlyVolume;

                StartofHour = StartofHour.AddMinutes(60 / binMultiplier);
            }
            return maxVolume;
        }

        protected double GetPeakHourDFactor(DateTime StartofHour, int Peakhourvolume, VolumeCollection volDic,
            int binMultiplier)
        {
            int totalVolume = 0;
            double PHDF = 0;

            for (int i = 0; i < binMultiplier; i++)
            {
                if (volDic.Items.Any(d => d.StartTime == StartofHour))
                    totalVolume = totalVolume + volDic.Items.FirstOrDefault(d => d.StartTime == StartofHour).HourlyVolume;

                StartofHour = StartofHour.AddMinutes(60 / binMultiplier);
            }
            totalVolume /= binMultiplier;
            totalVolume += Peakhourvolume;
            if (totalVolume > 0)
                PHDF = GetPeakHourKFactor(Convert.ToDouble(Peakhourvolume) / Convert.ToDouble(totalVolume), 3);
            else
                PHDF = 0;
            return PHDF;
        }
    }
}