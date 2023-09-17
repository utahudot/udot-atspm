using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Google.Type;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class GreenTimeUtilizationService
    {
        //define ECs
        private const int PHASE_BEGIN_GREEN = 1;
        private const int PHASE_BEGIN_YELLOW = 8;
        private const int PHASE_END_RED_CLEAR = 11;
        private const int DETECTOR_ON = 82;

        private readonly ISignalRepository signalRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public GreenTimeUtilizationService(
            ISignalRepository signalRepository,
            IControllerEventLogRepository controllerEventLogRepository)
        {
            this.signalRepository = signalRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        public GreenTimeUtilizationResult GreenTimeUtilizationService(
            Approach approach,
            GreenTimeUtilizationOptions options,
            bool getPermissivePhase) // the plans/splits input is still TBD
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();

            GreenTimeUtilizationResult result = new GreenTimeUtilizationResult(
                approach.Id,
                approach.Signal.SignalIdentifier,
                options.Start,
                options.End,
                //
                );

            //define properties
            var phaseNumberSort = getPermissivePhase ? approach.PermissivePhaseNumber.Value.ToString() + "-1" : approach.ProtectedPhaseNumber.ToString() + "-2";

            var phaseNumber = getPermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber; //this and the if statement below were taken from ChartTitleFactory.GetPhaseAndPhaseDescriptions

            string phaseNumberDescription;
            if ((approach.IsProtectedPhaseOverlap && !getPermissivePhase) ||
                (approach.IsPermissivePhaseOverlap && getPermissivePhase))
            {
                phaseNumberDescription = "Overlap " + phaseNumber + ": " + approach.Description;
            }
            else
            {
                phaseNumberDescription = "Phase " + phaseNumber + ": " + approach.Description;
            }

            //get a list of cycle events
            var phaseEventNumbers = new List<int> { PHASE_BEGIN_GREEN, PHASE_BEGIN_YELLOW };
            var phaseEvents = controllerEventLogs.GetCycleEventsWithTimeExtension(approach, options.UsePermissivePhase, options.Start, options.End)
            var checkAgainstEvents = new List<ControllerEventLog>();
            if (getPermissivePhase == true && approach.ProtectedPhaseNumber != 0)   // if it's a permissive phase, it will need to be checked against the protected green/yellow events
            {
                checkAgainstEvents = cel.GetEventsByEventCodesParam(options.SignalID, options.StartDate, options.EndDate.AddMinutes(options.SelectedAggSize), phaseEventNumbers, approach.ProtectedPhaseNumber);
            }

            //get a list of detections for that phase
            var detectorsToUse = approach.GetAllDetectorsOfDetectionType(4);  //should this really be approach-based and not phase-based? - I think so because of getpermissivephase
            var allDetectionEvents = cel.GetSignalEventsByEventCode(options.SignalID, options.StartDate, options.EndDate.AddMinutes(options.SelectedAggSize), DETECTOR_ON);
            var detectionEvents = new List<Controller_Event_Log>();
            foreach (var detector in detectorsToUse)
            {
                detectionEvents.AddRange(allDetectionEvents.Where(x =>
                    x.EventCode == DETECTOR_ON && x.EventParam == detector.DetChannel));
            }

            //loop for each Agg bin
            for (DateTime StartAggTime = options.StartDate; StartAggTime < options.EndDate; StartAggTime = StartAggTime.AddMinutes(options.SelectedAggSize))
            {
                DateTime endAggTime = StartAggTime.AddMinutes(options.SelectedAggSize) <= options.EndDate ? StartAggTime.AddMinutes(options.SelectedAggSize) : options.EndDate; //make the enddate the end of the bin or the end of the ananlysis period, whichever is sooner
                List<double> greenDurationList = new List<double>();
                List<int> BinValueList = new List<int>(new int[99]);
                int cycleCount = 0;

                //determine timestamps of the first green and last yellow
                var firstGreen = phaseEvents.Where(x => x.Timestamp > StartAggTime && x.EventCode == PHASE_BEGIN_GREEN).OrderBy(x => x.Timestamp).FirstOrDefault();
                if (firstGreen is null || firstGreen.Timestamp > endAggTime)
                {
                    continue; //skip this agg and go to the next if there is no green at all or if there isw no green in the agg period
                }
                var lastGreen = phaseEvents.Where(x => x.Timestamp < endAggTime && x.EventCode == PHASE_BEGIN_GREEN).OrderByDescending(x => x.Timestamp).FirstOrDefault();
                var lastYellow = phaseEvents.Where(x => x.Timestamp > lastGreen.Timestamp && x.EventCode == PHASE_BEGIN_YELLOW).OrderBy(x => x.Timestamp).FirstOrDefault();

                //get the event lists for the agg bin
                var aggDetections = detectionEvents
                    .Where(x => x.Timestamp >= firstGreen.Timestamp &&
                                x.Timestamp <= lastYellow.Timestamp)
                    .OrderBy(x => x.Timestamp);
                var greenList = phaseEvents
                        .Where(x => x.EventCode == PHASE_BEGIN_GREEN &&
                                    x.Timestamp >= firstGreen.Timestamp &&
                                    x.Timestamp <= lastGreen.Timestamp)
                        .OrderBy(x => x.Timestamp);
                var yellowList = phaseEvents
                    .Where(x => x.EventCode == PHASE_BEGIN_YELLOW &&
                                x.Timestamp >= firstGreen.Timestamp &&
                                x.Timestamp <= lastYellow.Timestamp)
                    .OrderBy(x => x.Timestamp);
                if (getPermissivePhase && checkAgainstEvents != null)
                {
                    var yProtectedEvents = checkAgainstEvents.Where(x => x.EventCode == PHASE_BEGIN_YELLOW);
                    var gProtectedEvents = checkAgainstEvents.Where(x => x.EventCode == PHASE_BEGIN_GREEN);
                    greenList = (IOrderedEnumerable<Controller_Event_Log>)CheckProtectedGreens(greenList, yellowList, gProtectedEvents, yProtectedEvents); //redefine the greenList after editng the green values to start at the beginning of the protected yellow phases if there is overlapping green time between the protected nad permissive phases (only happens with doghouses)
                }

                //pair each green with a yellow
                foreach (var green in greenList)
                {
                    //Find the corresponding yellow
                    var yellow = yellowList.Where(x => x.Timestamp >= green.Timestamp).OrderBy(x => x.Timestamp)
                        .FirstOrDefault();
                    if (yellow == null)
                        continue;

                    //get the green duration
                    TimeSpan greenDuration = yellow.Timestamp - green.Timestamp;
                    greenDurationList.Add(greenDuration.TotalSeconds);  //if zero, maybe don't add it?  

                    //count the number of cycles
                    cycleCount++;

                    //Find all events between the green and yellow
                    var greenDetectionsList = aggDetections
                        .Where(x => x.Timestamp >= green.Timestamp && x.Timestamp < yellow.Timestamp)
                        .OrderBy(x => x.Timestamp).ToList();
                    if (!greenDetectionsList.Any())
                        continue;
                    //maybe add a catch here where you make sure the BinValue list has as many indexes as it would need to cover the whole green (and add more if not)
                    //****or maybe add a catch inside the foreach loop below that checks if the intended index exists - and add enough to make sure it does ****

                    //add 1 to the bin value for each detection occuring during green
                    foreach (var detection in greenDetectionsList)
                    {
                        TimeSpan timeSinceGreenStart = detection.Timestamp - green.Timestamp;
                        var binnumber = (int)(timeSinceGreenStart.TotalSeconds / options.SelectedBinSize);
                        if (BinValueList.Count < binnumber)
                        {
                            int howMany = binnumber - BinValueList.Count + 1;  //check the numbering on this, might need to add 1, etc)
                            for (int i = 1; i <= howMany; i++)
                            {
                                BinValueList.Add(0); //add a new value of 0 -- not sure if this is the right statement yet
                            }
                        }
                        BinValueList[binnumber] = BinValueList[binnumber] + 1;

                    }

                }

                //create new classes
                Stacks.Add(new BarStack(StartAggTime, BinValueList, cycleCount, options.SelectedBinSize));
                AvgSplits.Add(new AverageSplit(StartAggTime, greenDurationList));
            }

            //get plans
            var plans = PlanFactory.GetSplitMonitorPlans(options.StartDate, options.EndDate, SignalID);
            GetYellowRedTime(approach, options, phaseNumber);
            foreach (Plan analysisplan in plans)
            {
                //GetProgrammedSplitTimesInAnalysisPeriod(approach.ProtectedPhaseNumber, analysisplan, options.EndDate);
                GetProgrammedSplitTime(phaseNumber, analysisplan.StartTime, analysisplan.EndTime.AddMinutes(-1));
                ProgSplits.Add(new ProgrammedSplit(analysisplan, options.StartDate, splitLength, durYellowRed));
            }


        }


        void GetProgrammedSplitTime(int phaseNumber, DateTime startDate, DateTime endDate)
        {
            SPM db = new SPM();
            var cel = ControllerEventLogRepositoryFactory.Create(db);
            GetEventCodeForPhase(phaseNumber);
            var tempSplitTimes = cel.GetSignalEventsByEventCode(SignalID, startDate.Date, endDate, splitLengthEventCode)
                .OrderByDescending(e => e.Timestamp).ToList();
            foreach (var tempSplitTime in tempSplitTimes)
            {
                if (tempSplitTime.Timestamp <= startDate)
                {
                    splitLength = tempSplitTime.EventParam;
                    break;
                }
            }
        }



        void GetProgrammedSplitTimesInAnalysisPeriod(int phaseNumber, Plan analysisplan, DateTime analysisEnd)
        {
            SPM db = new SPM();
            var cel = ControllerEventLogRepositoryFactory.Create(db);
            GetEventCodeForPhase(phaseNumber);
            var tempSplitTimes = cel.GetSignalEventsByEventCode(SignalID, analysisplan.StartTime, analysisEnd, splitLengthEventCode)
                .OrderByDescending(e => e.Timestamp).ToList();
            int i = 0;
            for (i = 0; tempSplitTimes[i].Timestamp < analysisplan.StartTime; i++)
            {
                splitLength = tempSplitTimes[i].EventParam;
                break;

            }
            i++;

        }


        void GetEventCodeForPhase(int PhaseNumber)
        {
            switch (PhaseNumber)
            {
                case 1:
                    splitLengthEventCode = 134;
                    break;
                case 2:
                    splitLengthEventCode = 135;
                    break;
                case 3:
                    splitLengthEventCode = 136;
                    break;
                case 4:
                    splitLengthEventCode = 137;
                    break;
                case 5:
                    splitLengthEventCode = 138;
                    break;
                case 6:
                    splitLengthEventCode = 139;
                    break;
                case 7:
                    splitLengthEventCode = 140;
                    break;
                case 8:
                    splitLengthEventCode = 141;
                    break;
                case 17:
                    splitLengthEventCode = 203;
                    break;
                case 18:
                    splitLengthEventCode = 204;
                    break;
                case 19:
                    splitLengthEventCode = 205;
                    break;
                case 20:
                    splitLengthEventCode = 206;
                    break;
                case 21:
                    splitLengthEventCode = 207;
                    break;
                case 22:
                    splitLengthEventCode = 208;
                    break;
                case 23:
                    splitLengthEventCode = 209;
                    break;
                case 24:
                    splitLengthEventCode = 210;
                    break;
                case 25:
                    splitLengthEventCode = 211;
                    break;
                case 26:
                    splitLengthEventCode = 212;
                    break;
                case 27:
                    splitLengthEventCode = 213;
                    break;
                case 28:
                    splitLengthEventCode = 214;
                    break;
                case 29:
                    splitLengthEventCode = 215;
                    break;
                case 30:
                    splitLengthEventCode = 216;
                    break;
                case 31:
                    splitLengthEventCode = 217;
                    break;
                case 32:
                    splitLengthEventCode = 218;
                    break;
                default:
                    splitLengthEventCode = 219;
                    break;
            }
        }

        void GetYellowRedTime(Approach approach, GreenTimeUtilizationOptions options, int phaseNumber)
        {
            SPM db = new SPM();
            var cel = ControllerEventLogRepositoryFactory.Create(db);
            var yrEventNumbers = new List<int> { PHASE_BEGIN_YELLOW, PHASE_END_RED_CLEAR };
            var yrEvents = cel.GetEventsByEventCodesParam(options.SignalID, options.StartDate, options.EndDate, yrEventNumbers, phaseNumber);
            var yellowList = yrEvents.Where(x => x.EventCode == PHASE_BEGIN_YELLOW)
                .OrderBy(x => x.Timestamp);
            var redList = yrEvents.Where(x => x.EventCode == PHASE_END_RED_CLEAR)
                .OrderBy(x => x.Timestamp);
            var startyellow = yellowList.FirstOrDefault();
            var endRedClear = redList.Where(x => x.Timestamp > startyellow.Timestamp).OrderBy(x => x.Timestamp)
                    .FirstOrDefault();
            if (startyellow is null || endRedClear is null)
            {
                durYellowRed = 0;
            }
            else
            {
                TimeSpan spanYellowRed = endRedClear.Timestamp - startyellow.Timestamp;
                durYellowRed = spanYellowRed.TotalSeconds;
            }
        }
        IEnumerable<Controller_Event_Log> CheckProtectedGreens(IEnumerable<Controller_Event_Log> gPermissiveEvents, IEnumerable<Controller_Event_Log> yPermissiveEvents, IEnumerable<Controller_Event_Log> gProtectedEvents, IEnumerable<Controller_Event_Log> yProtectedEvents)
        {
            foreach (var gPermissive in gPermissiveEvents)
            {
                var gProtected = gProtectedEvents.Where(x => x.Timestamp == gPermissive.Timestamp).OrderBy(x => x.Timestamp)
                .FirstOrDefault();
                if (gProtected == null)
                {
                    continue;
                }
                var yProtected = yProtectedEvents.Where(x => x.Timestamp > gProtected.Timestamp).OrderBy(x => x.Timestamp)
                .FirstOrDefault();
                if (yProtected == null)
                {
                    continue;
                }
                var yPermissive = yPermissiveEvents.Where(x => x.Timestamp > gPermissive.Timestamp).OrderBy(x => x.Timestamp)
                    .FirstOrDefault();
                if (yPermissive.Timestamp > yProtected.Timestamp)
                {
                    gPermissive.Timestamp = yProtected.Timestamp;
                }
                else
                {
                    gPermissive.Timestamp = yPermissive.Timestamp;
                }
            }

            return gPermissiveEvents.OrderBy(x => x.Timestamp);
        }


    }
}
}
