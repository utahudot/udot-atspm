﻿using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.TempExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using DateTime = System.DateTime;

namespace ATSPM.ReportApi.Business.GreenTimeUtilization
{
    public class GreenTimeUtilizationService
    {
        //define ECs
        private const int PHASE_BEGIN_GREEN = 1;
        private const int PHASE_BEGIN_YELLOW = 8;
        private const int PHASE_END_RED_CLEAR = 11;
        private readonly PlanService planService;

        public GreenTimeUtilizationService(
            PlanService planService)
        {
            this.planService = planService;
        }

        public GreenTimeUtilizationResult GetChartData(
            PhaseDetail phaseDetail,
            GreenTimeUtilizationOptions options,
            List<ControllerEventLog> detectorEvents, //DetectorType 4
            List<ControllerEventLog> cycleEvents, //PHASE_BEGIN_GREEN, PHASE_BEGIN_YELLOW
            List<ControllerEventLog> planEvents,
            List<ControllerEventLog> controllerEventLogs
            ) // the plans/splits input is still TBD
        {
            //var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            //var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();


            var isPermissivePhase = phaseDetail.PhaseNumber != phaseDetail.Approach.ProtectedPhaseNumber;
            //define properties
            string phaseNumberSort;
            if (phaseDetail.Approach.PermissivePhaseNumber.HasValue)
            {
                phaseNumberSort = phaseDetail.PhaseNumber == phaseDetail.Approach.PermissivePhaseNumber.Value ?
                   phaseDetail.Approach.PermissivePhaseNumber.Value.ToString() + "-1" : phaseDetail.Approach.ProtectedPhaseNumber.ToString() + "-2";
            }
            else
            {
                phaseNumberSort = phaseDetail.Approach.ProtectedPhaseNumber.ToString() + "-2";
            }

            string phaseNumberDescription;
            if (phaseDetail.UseOverlap)
            {
                phaseNumberDescription = "Overlap " + phaseDetail.PhaseNumber + ": " + phaseDetail.Approach.Description;
            }
            else
            {
                phaseNumberDescription = "Phase " + phaseDetail.PhaseNumber + ": " + phaseDetail.Approach.Description;
            }

            //get a list of cycle events
            var phaseEventNumbers = new List<int> { PHASE_BEGIN_GREEN, PHASE_BEGIN_YELLOW };
            //var phaseEvents = controllerEventLogs.GetCycleEventsWithTimeExtension(approach, options.UsePermissivePhase, options.Start, options.End)
            var checkAgainstEvents = new List<ControllerEventLog>();
            if (isPermissivePhase == true && phaseDetail.PhaseNumber != 0)   // if it's a permissive phase, it will need to be checked against the protected green/yellow events
            {
                checkAgainstEvents = controllerEventLogs.GetEventsByEventCodes(options.Start, options.End.AddMinutes(options.XAxisBinSize), phaseEventNumbers, phaseDetail.PhaseNumber).ToList();
            }

            //get a list of detections for that phase
            //var detectorsToUse = approach.GetAllDetectorsOfDetectionType(4);  //should this really be approach-based and not phase-based? - I think so because of getpermissivephase
            //var allDetectionEvents = cel.GetSignalEventsByEventCode(options.SignalIdentifier, options.Start, options.End.AddMinutes(options.SelectedAggSize), DETECTOR_ON);
            //var detectionEvents = new List<ControllerEventLog>();
            //foreach (var detector in detectorsToUse)
            //{
            //    detectionEvents.AddRange(allDetectionEvents.Where(x =>
            //        x.EventCode == DETECTOR_ON && x.EventParam == detector.DetectorChannel));
            //}

            var bins = new List<BarStack>();
            var averageSplits = new List<AverageSplit>();
            int xAxisBinNumber = 0;
            //loop for each Agg bin
            for (var StartBinTime = options.Start; StartBinTime < options.End; StartBinTime = StartBinTime.AddMinutes(options.XAxisBinSize), xAxisBinNumber++)
            {
                DateTime endAggTime = StartBinTime.AddMinutes(options.XAxisBinSize) <= options.End ? StartBinTime.AddMinutes(options.XAxisBinSize) : options.End; //make the enddate the end of the bin or the end of the ananlysis period, whichever is sooner
                List<double> greenDurationList = new List<double>();
                List<int> BinValueList = new List<int>(new int[99]);
                int cycleCount = 0;

                //determine timestamps of the first green and last yellow
                var firstGreen = cycleEvents.Where(x => x.Timestamp > StartBinTime && x.EventCode == PHASE_BEGIN_GREEN).OrderBy(x => x.Timestamp).FirstOrDefault();
                if (firstGreen is null || firstGreen.Timestamp > endAggTime)
                {
                    continue; //skip this agg and go to the next if there is no green at all or if there isw no green in the agg period
                }
                var lastGreen = cycleEvents.Where(x => x.Timestamp < endAggTime && x.EventCode == PHASE_BEGIN_GREEN).OrderByDescending(x => x.Timestamp).FirstOrDefault();
                var lastYellow = cycleEvents.Where(x => x.Timestamp > lastGreen.Timestamp && x.EventCode == PHASE_BEGIN_YELLOW).OrderBy(x => x.Timestamp).FirstOrDefault();
                if (lastGreen is null || lastYellow is null)
                {
                    continue; //skip this agg and go to the next if there is no green at all or if there isw no green in the agg period
                }

                //get the event lists for the agg bin

                var aggDetections = detectorEvents
                    .Where(x => x.Timestamp >= firstGreen.Timestamp &&
                                x.Timestamp <= lastYellow.Timestamp)
                    .OrderBy(x => x.Timestamp);
                var greenList = cycleEvents
                        .Where(x => x.EventCode == PHASE_BEGIN_GREEN &&
                                    x.Timestamp >= firstGreen.Timestamp &&
                                    x.Timestamp <= lastGreen.Timestamp)
                        .OrderBy(x => x.Timestamp);
                var yellowList = cycleEvents
                    .Where(x => x.EventCode == PHASE_BEGIN_YELLOW &&
                                x.Timestamp >= firstGreen.Timestamp &&
                                x.Timestamp <= lastYellow.Timestamp)
                    .OrderBy(x => x.Timestamp);
                if (isPermissivePhase && checkAgainstEvents != null)
                {
                    var yProtectedEvents = checkAgainstEvents.Where(x => x.EventCode == PHASE_BEGIN_YELLOW);
                    var gProtectedEvents = checkAgainstEvents.Where(x => x.EventCode == PHASE_BEGIN_GREEN);
                    greenList = (IOrderedEnumerable<ControllerEventLog>)CheckProtectedGreens(greenList, yellowList, gProtectedEvents, yProtectedEvents); //redefine the greenList after editng the green values to start at the beginning of the protected yellow phases if there is overlapping green time between the protected nad permissive phases (only happens with doghouses)
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
                        var yAxisBinNumber = (int)(timeSinceGreenStart.TotalSeconds / options.XAxisBinSize);
                        if (BinValueList.Count < yAxisBinNumber)
                        {
                            int howMany = yAxisBinNumber - BinValueList.Count + 1;  //check the numbering on this, might need to add 1, etc)
                            for (int i = 1; i <= howMany; i++)
                            {
                                BinValueList.Add(0); //add a new value of 0 -- not sure if this is the right statement yet
                            }
                        }
                        BinValueList[yAxisBinNumber] = BinValueList[yAxisBinNumber] + 1;

                    }
                }

                createStack(bins, BinValueList, cycleCount, xAxisBinNumber);
                averageSplits.Add(new AverageSplit(StartBinTime, greenDurationList));
            }

            //get plans
            var plans = planService.GetSplitMonitorPlans(options.Start, options.End, options.SignalIdentifier, planEvents);
            var durYellowRed = GetYellowRedTimeSeconds(options, phaseDetail.PhaseNumber, cycleEvents);
            var programmedSplits = new List<ProgrammedSplit>();
            foreach (Plan analysisplan in plans)
            {
                //GetProgrammedSplitTimesInAnalysisPeriod(approach.ProtectedPhaseNumber, analysisplan, options.EndDate);
                var splitTime = GetProgrammedSplitTime(phaseDetail.PhaseNumber, analysisplan.Start, analysisplan.End.AddMinutes(-1), controllerEventLogs);
                programmedSplits.Add(new ProgrammedSplit(analysisplan, options.Start, splitTime, durYellowRed));
            }

            GreenTimeUtilizationResult result = new GreenTimeUtilizationResult(
                phaseDetail.Approach.Id,
                phaseDetail.Approach.Signal.SignalIdentifier,
                options.Start,
                options.End,
                bins,
                averageSplits,
                programmedSplits,
                phaseDetail.PhaseNumber,
                phaseNumberSort
                );
            result.ApproachDescription = phaseDetail.Approach.Description;
            result.SignalDescription = phaseDetail.Approach.Signal.SignalDescription();
            return result;
        }

        private void createStack(List<BarStack> bins, List<int> binValueList, int cycleCount, int xAxisBinNumber)
        {
            int maxI = 0;
            for (int i = 0; i < binValueList.Count; i++)
            {
                if (binValueList[i] != 0 && i > maxI)
                {
                    maxI = i;
                }
            }

            for (int yAxisBinNumber = 0; yAxisBinNumber <= maxI; yAxisBinNumber++)
            {
                double value = Math.Round(binValueList[yAxisBinNumber] / (double)cycleCount, 2);
                bins.Add(new BarStack(xAxisBinNumber, yAxisBinNumber, value));
            }
        }

        private int GetProgrammedSplitTime(
            int phaseNumber,
            DateTime startDate,
            DateTime endDate,
            List<ControllerEventLog> controllerEventLogs)
        {
            var eventCode = GetEventCodeForPhase(phaseNumber);
            return controllerEventLogs.GetEventsByEventCodes(startDate, endDate, new List<int> { eventCode })
                .OrderByDescending(e => e.Timestamp)
                .Where(e => e.Timestamp <= startDate)
                .Select(e => e.EventParam)
                .FirstOrDefault();
        }



        //void GetProgrammedSplitTimesInAnalysisPeriod(int phaseNumber, Plan analysisplan, DateTime analysisEnd)
        //{
        //    SPM db = new SPM();
        //    var cel = ControllerEventLogRepositoryFactory.Create(db);
        //    GetEventCodeForPhase(phaseNumber);
        //    var tempSplitTimes = cel.GetSignalEventsByEventCode(SignalID, analysisplan.StartTime, analysisEnd, splitLengthEventCode)
        //        .OrderByDescending(e => e.Timestamp).ToList();
        //    int i = 0;
        //    for (i = 0; tempSplitTimes[i].Timestamp < analysisplan.StartTime; i++)
        //    {
        //        splitLength = tempSplitTimes[i].EventParam;
        //        break;

        //    }
        //    i++;

        //}


        private int GetEventCodeForPhase(int PhaseNumber)
        {
            switch (PhaseNumber)
            {
                case 1:
                    return 134;
                case 2:
                    return 135;
                case 3:
                    return 136;
                case 4:
                    return 137;
                case 5:
                    return 138;
                case 6:
                    return 139;
                case 7:
                    return 140;
                case 8:
                    return 141;
                case 17:
                    return 203;
                case 18:
                    return 204;
                case 19:
                    return 205;
                case 20:
                    return 206;
                case 21:
                    return 207;
                case 22:
                    return 208;
                case 23:
                    return 209;
                case 24:
                    return 210;
                case 25:
                    return 211;
                case 26:
                    return 212;
                case 27:
                    return 213;
                case 28:
                    return 214;
                case 29:
                    return 215;
                case 30:
                    return 216;
                case 31:
                    return 217;
                case 32:
                    return 218;
                default:
                    return 219;
            }
        }

        private double GetYellowRedTimeSeconds(
            GreenTimeUtilizationOptions options,
            int phaseNumber,
            List<ControllerEventLog> cycleEvents)
        {
            var yrEventNumbers = new List<int> { PHASE_BEGIN_YELLOW, PHASE_END_RED_CLEAR };
            var yrEvents = cycleEvents.GetEventsByEventCodes(options.Start, options.End, yrEventNumbers, phaseNumber);
            var yellowList = yrEvents.Where(x => x.EventCode == PHASE_BEGIN_YELLOW)
                .OrderBy(x => x.Timestamp);
            var redList = yrEvents.Where(x => x.EventCode == PHASE_END_RED_CLEAR)
                .OrderBy(x => x.Timestamp);
            var startyellow = yellowList.FirstOrDefault();
            var endRedClear = redList.Where(x => x.Timestamp > startyellow.Timestamp).OrderBy(x => x.Timestamp)
                    .FirstOrDefault();
            if (startyellow is null || endRedClear is null)
            {
                return 0;
            }
            else
            {
                TimeSpan spanYellowRed = endRedClear.Timestamp - startyellow.Timestamp;
                return spanYellowRed.TotalSeconds;
            }
        }
        IEnumerable<ControllerEventLog> CheckProtectedGreens(IEnumerable<ControllerEventLog> gPermissiveEvents, IEnumerable<ControllerEventLog> yPermissiveEvents, IEnumerable<ControllerEventLog> gProtectedEvents, IEnumerable<ControllerEventLog> yProtectedEvents)
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
