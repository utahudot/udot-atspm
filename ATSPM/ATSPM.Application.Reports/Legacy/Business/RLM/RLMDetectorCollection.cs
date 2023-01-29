//using ATSPM.Application.Repositories;
//using ATSPM.Application.Extensions;
//using ATSPM.Data.Models;
//using System;
//using System.Collections.Generic;

//namespace Legacy.Common.Business
//{
//    public class RedLightMonitorDetectorCollection
//    {
//        private List<DetectorData> NBdetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> NBDetectors
//        {
//            get { return NBdetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> SBdetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> SBDetectors
//        {
//            get { return SBdetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> EBdetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> EBDetectors
//        {
//            get { return EBdetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> WBdetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> WBDetectors
//        {
//            get { return WBdetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> NBbikedetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> NBBikeDetectors
//        {
//            get { return NBbikedetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> SBbikedetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> SBBikeDetectors
//        {
//            get { return SBbikedetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> EBbikedetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> EBBikeDetectors
//        {
//            get { return EBbikedetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> WBbikedetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> WBBikeDetectors
//        {
//            get { return WBbikedetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> NBpeddetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> NBPedDetectors
//        {
//            get { return NBpeddetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> SBpeddetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> SBPedDetectors
//        {
//            get { return SBpeddetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> EBpeddetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> EBPedDetectors
//        {
//            get { return EBpeddetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> WBpeddetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> WBPedDetectors
//        {
//            get { return WBpeddetectors; }
//        }

//        private List<List<DetectorData>> pedDetectors = new List<List<DetectorData>>();
//        public List<List<DetectorData>> PedDetectors
//        {
//            get { return PedDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> NBleftDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> NBLeftDetectors
//        {
//            get { return NBleftDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> SBleftDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> SBLeftDetectors
//        {
//            get { return SBleftDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> EBleftDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> EBLeftDetectors
//        {
//            get { return EBleftDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> WBleftDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> WBLeftDetectors
//        {
//            get { return WBleftDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> NBrightDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> NBRightDetectors
//        {
//            get { return NBrightDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> SBrightDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> SBRightDetectors
//        {
//            get { return SBrightDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> EBrightDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> EBRightDetectors
//        {
//            get { return EBrightDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> WBrightDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> WBRightDetectors
//        {
//            get { return WBrightDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> NBthruDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> NBThruDetectors
//        {
//            get { return NBthruDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> SBthruDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> SBThruDetectors
//        {
//            get { return SBthruDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> EBthruDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> EBThruDetectors
//        {
//            get { return EBthruDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> WBthruDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> WBThruDetectors
//        {
//            get { return WBthruDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> NBexitDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> NBExitDetectors
//        {
//            get { return NBexitDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> SBexitDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> SBExitDetectors
//        {
//            get { return SBexitDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> EBexitDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> EBExitDetectors
//        {
//            get { return EBexitDetectors; }
//        }

//        private List<Legacy.Common.Business.DetectorData> WBexitDetectors = new List<DetectorData>();
//        public List<Legacy.Common.Business.DetectorData> WBExitDetectors
//        {
//            get { return WBexitDetectors; }
//        }

//        private List<DetectorData> NBLTbikeDetectors = new List<DetectorData>();
//        public List<DetectorData> NBLTBikeDetectors
//        {
//            get { return NBLTbikeDetectors; }
//        }

//        private List<DetectorData> NBRTbikeDetectors = new List<DetectorData>();
//        public List<DetectorData> NBRTBikeDetectors
//        {
//            get { return NBRTbikeDetectors; }
//        }

//        private List<DetectorData> NBThrubikeDetectors = new List<DetectorData>();
//        public List<DetectorData> NBThruBikeDetectors
//        {
//            get { return NBThrubikeDetectors; }
//        }

//        private List<DetectorData> SBLTbikeDetectors = new List<DetectorData>();
//        public List<DetectorData> SBLTBikeDetectors
//        {
//            get { return SBLTbikeDetectors; }
//        }

//        private List<DetectorData> SBRTbikeDetectors = new List<DetectorData>();
//        public List<DetectorData> SBRTBikeDetectors
//        {
//            get { return SBRTbikeDetectors; }
//        }

//        private List<DetectorData> SBThrubikeDetectors = new List<DetectorData>();
//        public List<DetectorData> SBThruBikeDetectors
//        {
//            get { return SBThrubikeDetectors; }
//        }

//        private List<DetectorData> EBLTbikeDetectors = new List<DetectorData>();
//        public List<DetectorData> EBLTBikeDetectors
//        {
//            get { return EBLTbikeDetectors; }
//        }

//        private List<DetectorData> EBRTbikeDetectors = new List<DetectorData>();
//        public List<DetectorData> EBRTBikeDetectors
//        {
//            get { return EBRTbikeDetectors; }
//        }

//        private List<DetectorData> EBThrubikeDetectors = new List<DetectorData>();
//        public List<DetectorData> EBThruBikeDetectors
//        {
//            get { return EBThrubikeDetectors; }
//        }

//        private List<DetectorData> WBLTbikeDetectors = new List<DetectorData>();
//        public List<DetectorData> WBLTBikeDetectors
//        {
//            get { return WBLTbikeDetectors; }
//        }

//        private List<DetectorData> WBRTbikeDetectors = new List<DetectorData>();
//        public List<DetectorData> WBRTBikeDetectors
//        {
//            get { return WBRTbikeDetectors; }
//        }

//        private List<DetectorData> WBThrubikeDetectors = new List<DetectorData>();
//        public List<DetectorData> WBThruBikeDetectors
//        {
//            get { return WBThrubikeDetectors; }
//        }

//        private string signalId;
//        public string SignalId
//        {
//            get { return signalId; }
//        }

//        public List<Legacy.Common.Business.DetectorData> DetectorsForRLM = new List<DetectorData>();
//        private readonly ISignalRepository signalRepository;

//        /// <summary>
//        /// Default constructor for the DetectorCollection used in the Turning Movement Counts charts
//        /// </summary>
//        /// <param name="signalID"></param>
//        /// <param name="start"></param>
//        /// <param name="end"></param>
//        /// <param name="binsize"></param>
//        public RedLightMonitorDetectorCollection(DateTime start, DateTime end, int binsize,
//            Approach approach)
//        {
//            var dets = approach.GetDetectorsForMetricType(11);
//            foreach (Detector detector in dets)
//            {
//                DetectorData Detector = new DetectorData(detector, start, end, binsize);
//                DetectorsForRLM.Add(Detector);
//            }
//            //SortDetectors();
//        }


//        /// <summary>
//        /// Alternate Constructor for PDC type data.
//        /// </summary>
//        /// <param name="signalid"></param>
//        /// <param name="ApproachDirection"></param>
//        public RedLightMonitorDetectorCollection(string signalID, string ApproachDirection)
//        {
//            var signal = signalRepository.Lookup(signalID);
//            List<Detector> detectors = GetDetectorsForSignalThatSupportAMetricByApproachDirection(11, ApproachDirection);

//            foreach (Detector detector in detectors)
//            {
//                DetectorData Detector = new DetectorData(detector, detector.DetectorId.ToString(), signalID, detector.DetChannel, detector.Approach);
//                DetectorsForRLM.Add(Detector);
//            }
//        }

//        public RedLightMonitorDetectorCollection(ISignalRepository signalRepository)
//        {
//            this.signalRepository = signalRepository;
//        }

//        public Legacy.Common.Business.ControllerEventLogService CombineDetectorData(DateTime startDate, DateTime endDate, string signalId)
//        {
//            Legacy.Common.Models.SPM db = new Legacy.Common.Models.SPM();
//            Legacy.Common.Business.ControllerEventLogService detectortable =
//                new Legacy.Common.Business.ControllerEventLogService(signalId, startDate, endDate);
//            List<Legacy.Common.Business.ControllerEventLogService> Tables = new List<Legacy.Common.Business.ControllerEventLogService>();
//            foreach (Legacy.Common.Business.DetectorData Detector in DetectorsForRLM)
//            {
//                Legacy.Common.Business.ControllerEventLogService TEMPdetectortable =
//                    new Legacy.Common.Business.ControllerEventLogService(signalId, startDate, endDate, Detector.Channel,
//                        new List<int>() { 82 });
//                Tables.Add(TEMPdetectortable);
//            }
//            foreach (Legacy.Common.Business.ControllerEventLogService Table in Tables)
//            {
//                detectortable.MergeEvents(Table);
//            }
//            return detectortable;
//        }

//        public List<Detector> GetDetectorsForSignalThatSupportAMetricByApproachDirection(int MetricTypeID,
//            string Direction)
//        {
//            var gdr =
//                DetectorRepositoryFactory.Create();
//            var detectors = new List<Detector>();
//            foreach (var d in GetDetectorsForSignal())
//                if (gdr.CheckReportAvialbility(d.DetectorID, MetricTypeID) &&
//                    d.Approach.DirectionType.Description == Direction)
//                    detectors.Add(d);
//            return detectors;
//        }



//    }
//}

