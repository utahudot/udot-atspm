using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Legacy.Common.Business.PEDDelay;
using System.ComponentModel.DataAnnotations;
using Legacy.Common.Models;
using System.Linq;
using System.Data;
using Legacy.Common.Models.Repositories;

namespace Legacy.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class PedDelayOptions : MetricOptions
    {
        public PedDelayOptions(string signalId, DateTime startDate, DateTime endDate, int timeBuffer, bool showPedBeginWalk, bool showCycleLength, bool showPercentDelay, bool showPedRecall, int pedRecallThreshold, double? yAxisMax)
        {
            SignalId = signalId;
            StartDate = startDate;
            EndDate = endDate;
            TimeBuffer = timeBuffer;
            ShowPedBeginWalk = showPedBeginWalk;
            ShowCycleLength = showCycleLength;
            ShowPercentDelay = showPercentDelay;
            ShowPedRecall = showPedRecall;
            PedRecallThreshold = pedRecallThreshold;
            YAxisMax = yAxisMax;
        }

        public PedDelayOptions()
        {
            Y2AxisMax = 10;
            SetDefaults();
        }

        [DataMember]
        [Display(Name = "Time Buffer Between Unique Ped Detections")]
        public int TimeBuffer { get; set; }

        [DataMember]
        [Display(Name = "Show Ped Begin Walk")]
        public bool ShowPedBeginWalk { get; set; }

        [DataMember]
        [Display(Name = "Show Cycle Length")]
        public bool ShowCycleLength { get; set; }

        [DataMember]
        [Display(Name = "Show Percent Delay")]
        public bool ShowPercentDelay { get; set; }

        [DataMember]
        [Display(Name = "Show Ped Recall")]
        public bool ShowPedRecall { get; set; }

        [DataMember]
        [Display(Name = "Ped Recall Threshold (Percent)")]
        public int PedRecallThreshold { get; set; }

        public override List<string> CreateMetric()
        {
            base.CreateMetric();
            var signalRepository = SignalsRepositoryFactory.Create();
            Signal signal = signalRepository.GetVersionOfSignalByDate(SignalId, StartDate);

            var pedDelaySignal = new PedDelaySignal(signal, TimeBuffer, StartDate, EndDate);

            foreach (var pedPhase in pedDelaySignal.PedPhases)
                if (pedPhase.Cycles.Count > 0)
                {
                    var cycleLength = CycleService.GetRedToRedCycles(pedPhase.Approach, StartDate, EndDate);
                    var pdc = new PEDDelayChart(this, pedPhase, cycleLength);
                    var chart = pdc.Chart;
                    var chartName = CreateFileName();
                    chart.SaveImage(MetricFileLocation + chartName);
                    ReturnList.Add(MetricWebPath + chartName);
                }
            return ReturnList;
        }
    }
}