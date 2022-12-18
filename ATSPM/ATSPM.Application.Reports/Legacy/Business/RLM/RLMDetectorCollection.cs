using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Legacy.Common.Business
{
    public class RLMDetectorCollection
    {
        private List<Legacy.Common.Business.Detector> NBdetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> NBDetectors
        {
            get { return NBdetectors; }
        }

        private List<Legacy.Common.Business.Detector> SBdetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> SBDetectors
        {
            get { return SBdetectors; }
        }

        private List<Legacy.Common.Business.Detector> EBdetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> EBDetectors
        {
            get { return EBdetectors; }
        }

        private List<Legacy.Common.Business.Detector> WBdetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> WBDetectors
        {
            get { return WBdetectors; }
        }

        private List<Legacy.Common.Business.Detector> NBbikedetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> NBBikeDetectors
        {
            get { return NBbikedetectors; }
        }

        private List<Legacy.Common.Business.Detector> SBbikedetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> SBBikeDetectors
        {
            get { return SBbikedetectors; }
        }

        private List<Legacy.Common.Business.Detector> EBbikedetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> EBBikeDetectors
        {
            get { return EBbikedetectors; }
        }

        private List<Legacy.Common.Business.Detector> WBbikedetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> WBBikeDetectors
        {
            get { return WBbikedetectors; }
        }

        private List<Legacy.Common.Business.Detector> NBpeddetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> NBPedDetectors
        {
            get { return NBpeddetectors; }
        }

        private List<Legacy.Common.Business.Detector> SBpeddetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> SBPedDetectors
        {
            get { return SBpeddetectors; }
        }

        private List<Legacy.Common.Business.Detector> EBpeddetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> EBPedDetectors
        {
            get { return EBpeddetectors; }
        }

        private List<Legacy.Common.Business.Detector> WBpeddetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> WBPedDetectors
        {
            get { return WBpeddetectors; }
        }

        private List<List<Detector>> pedDetectors = new List<List<Detector>>();
        public List<List<Detector>> PedDetectors
        {
            get { return PedDetectors; }
        }

        private List<Legacy.Common.Business.Detector> NBleftDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> NBLeftDetectors
        {
            get { return NBleftDetectors; }
        }

        private List<Legacy.Common.Business.Detector> SBleftDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> SBLeftDetectors
        {
            get { return SBleftDetectors; }
        }

        private List<Legacy.Common.Business.Detector> EBleftDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> EBLeftDetectors
        {
            get { return EBleftDetectors; }
        }

        private List<Legacy.Common.Business.Detector> WBleftDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> WBLeftDetectors
        {
            get { return WBleftDetectors; }
        }

        private List<Legacy.Common.Business.Detector> NBrightDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> NBRightDetectors
        {
            get { return NBrightDetectors; }
        }

        private List<Legacy.Common.Business.Detector> SBrightDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> SBRightDetectors
        {
            get { return SBrightDetectors; }
        }

        private List<Legacy.Common.Business.Detector> EBrightDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> EBRightDetectors
        {
            get { return EBrightDetectors; }
        }

        private List<Legacy.Common.Business.Detector> WBrightDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> WBRightDetectors
        {
            get { return WBrightDetectors; }
        }

        private List<Legacy.Common.Business.Detector> NBthruDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> NBThruDetectors
        {
            get { return NBthruDetectors; }
        }

        private List<Legacy.Common.Business.Detector> SBthruDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> SBThruDetectors
        {
            get { return SBthruDetectors; }
        }

        private List<Legacy.Common.Business.Detector> EBthruDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> EBThruDetectors
        {
            get { return EBthruDetectors; }
        }

        private List<Legacy.Common.Business.Detector> WBthruDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> WBThruDetectors
        {
            get { return WBthruDetectors; }
        }

        private List<Legacy.Common.Business.Detector> NBexitDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> NBExitDetectors
        {
            get { return NBexitDetectors; }
        }

        private List<Legacy.Common.Business.Detector> SBexitDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> SBExitDetectors
        {
            get { return SBexitDetectors; }
        }

        private List<Legacy.Common.Business.Detector> EBexitDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> EBExitDetectors
        {
            get { return EBexitDetectors; }
        }

        private List<Legacy.Common.Business.Detector> WBexitDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> WBExitDetectors
        {
            get { return WBexitDetectors; }
        }

        private List<Legacy.Common.Business.Detector> NBLTbikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> NBLTBikeDetectors
        {
            get { return NBLTbikeDetectors; }
        }

        private List<Legacy.Common.Business.Detector> NBRTbikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> NBRTBikeDetectors
        {
            get { return NBRTbikeDetectors; }
        }

        private List<Legacy.Common.Business.Detector> NBThrubikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> NBThruBikeDetectors
        {
            get { return NBThrubikeDetectors; }
        }

        private List<Legacy.Common.Business.Detector> SBLTbikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> SBLTBikeDetectors
        {
            get { return SBLTbikeDetectors; }
        }

        private List<Legacy.Common.Business.Detector> SBRTbikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> SBRTBikeDetectors
        {
            get { return SBRTbikeDetectors; }
        }

        private List<Legacy.Common.Business.Detector> SBThrubikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> SBThruBikeDetectors
        {
            get { return SBThrubikeDetectors; }
        }

        private List<Legacy.Common.Business.Detector> EBLTbikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> EBLTBikeDetectors
        {
            get { return EBLTbikeDetectors; }
        }

        private List<Legacy.Common.Business.Detector> EBRTbikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> EBRTBikeDetectors
        {
            get { return EBRTbikeDetectors; }
        }

        private List<Legacy.Common.Business.Detector> EBThrubikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> EBThruBikeDetectors
        {
            get { return EBThrubikeDetectors; }
        }

        private List<Legacy.Common.Business.Detector> WBLTbikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> WBLTBikeDetectors
        {
            get { return WBLTbikeDetectors; }
        }

        private List<Legacy.Common.Business.Detector> WBRTbikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> WBRTBikeDetectors
        {
            get { return WBRTbikeDetectors; }
        }

        private List<Legacy.Common.Business.Detector> WBThrubikeDetectors = new List<Detector>();
        public List<Legacy.Common.Business.Detector> WBThruBikeDetectors
        {
            get { return WBThrubikeDetectors; }
        }

        private string signalId;
        public string SignalId
        {
            get { return signalId; }
        }

        public List<Legacy.Common.Business.Detector> DetectorsForRLM = new List<Detector>();

        /// <summary>
        /// Default constructor for the DetectorCollection used in the Turning Movement Counts charts
        /// </summary>
        /// <param name="signalID"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="binsize"></param>
        public  RLMDetectorCollection(DateTime startdate, DateTime enddate, int binsize, 
            Legacy.Common.Models.Approach approach)
        {
            var dets = approach.GetDetectorsForMetricType(11);
            foreach (Legacy.Common.Models.Detector detector in dets)
            {
                Legacy.Common.Business.Detector Detector = new Detector(detector, startdate, enddate, binsize);
                DetectorsForRLM.Add(Detector);
            }
            //SortDetectors();
        }


        /// <summary>
        /// Alternate Constructor for PDC type data.
        /// </summary>
        /// <param name="signalid"></param>
        /// <param name="ApproachDirection"></param>
        public RLMDetectorCollection(string signalID, string ApproachDirection)
        {
            Legacy.Common.Models.Repositories.ISignalsRepository repository =
                Legacy.Common.Models.Repositories.SignalsRepositoryFactory.Create();
            var signal = repository.GetSignalBySignalID(signalID);
            List<Legacy.Common.Models.Detector> dets = signal.GetDetectorsForSignalThatSupportAMetricByApproachDirection(11, ApproachDirection);

            foreach (Legacy.Common.Models.Detector row in dets)
            {
               // MOE.Common.Business.Detector Detector = new Detector(row.DetectorID.ToString(), signalID, row.Det_Channel, row.Lane.LaneType, ApproachDirection);
                Legacy.Common.Business.Detector Detector = new Detector(row.DetectorID.ToString(), signalID, row.DetChannel, row.Approach);
                DetectorsForRLM.Add(Detector);
            }

        }

        public Legacy.Common.Business.ControllerEventLogs CombineDetectorData(DateTime startDate, DateTime endDate, string signalId)
        {
            Legacy.Common.Models.SPM db = new Legacy.Common.Models.SPM();
            Legacy.Common.Business.ControllerEventLogs detectortable = 
                new Legacy.Common.Business.ControllerEventLogs(signalId, startDate, endDate);
            List<Legacy.Common.Business.ControllerEventLogs> Tables = new List<Legacy.Common.Business.ControllerEventLogs>();
            foreach (Legacy.Common.Business.Detector Detector in DetectorsForRLM)
            {
                Legacy.Common.Business.ControllerEventLogs TEMPdetectortable =
                    new Legacy.Common.Business.ControllerEventLogs(signalId, startDate, endDate, Detector.Channel, 
                        new List<int>() { 82 });
                Tables.Add(TEMPdetectortable);
            }
            foreach (Legacy.Common.Business.ControllerEventLogs Table in Tables)
            {
                detectortable.MergeEvents(Table);
            }
            return detectortable;
        }
        


    }
}

