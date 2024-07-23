using ATSPM.Application.Business.Common;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.GreenTimeUtilization
{
    public class GreenTimeUtilizationService
    {
        //define ECs
        private readonly PlanService planService;

        public GreenTimeUtilizationService(
            PlanService planService)
        {
            this.planService = planService;
        }

        public GreenTimeUtilizationResult GetChartData(
            PhaseDetail phaseDetail,
            GreenTimeUtilizationOptions options,
            List<IndianaEvent> detectorEvents, //DetectorType 4
            List<IndianaEvent> cycleEvents, //PHASE_BEGIN_GREEN, PHASE_BEGIN_YELLOW
            List<IndianaEvent> planEvents,
            List<IndianaEvent> controllerEventLogs
            ) // the plans/splits input is still TBD
        {
            var isPermissivePhase = phaseDetail.PhaseNumber != phaseDetail.Approach.ProtectedPhaseNumber;

            //get a list of cycle events
            var phaseEventNumbers = new List<short> { 1, 8 };
            var checkAgainstEvents = new List<IndianaEvent>();
            if (isPermissivePhase && phaseDetail.PhaseNumber != 0)   // if it's a permissive phase, it will need to be checked against the protected timeStamp/yellow events
            {
                checkAgainstEvents = controllerEventLogs.GetEventsByEventCodes(options.Start, options.End.AddMinutes(options.XAxisBinSize), phaseEventNumbers, phaseDetail.PhaseNumber).ToList();
            }

            var bins = new List<BarStack>();
            var averageSplits = new List<DataPointForDouble>();
            int xAxisBinNumber = 0;
            //loop for each Agg bin
            for (var StartBinTime = options.Start; StartBinTime < options.End; StartBinTime = StartBinTime.AddMinutes(options.XAxisBinSize), xAxisBinNumber++)
            {
                DateTime endAggTime = StartBinTime.AddMinutes(options.XAxisBinSize) <= options.End ? StartBinTime.AddMinutes(options.XAxisBinSize) : options.End; //make the enddate the end of the bin or the end of the ananlysis period, whichever is sooner
                List<double> greenDurationList = new List<double>();
                List<int> BinValueList = new List<int>(new int[99]);
                int cycleCount = 0;

                //determine timestamps of the first timeStamp and last yellow
                var firstGreen = cycleEvents.Where(x => x.Timestamp > StartBinTime && x.EventCode == 1).OrderBy(x => x.Timestamp).FirstOrDefault();
                if (firstGreen is null || firstGreen.Timestamp > endAggTime)
                {
                    continue; //skip this agg and go to the next if there is no timeStamp at all or if there isw no timeStamp in the agg period
                }
                var lastGreen = cycleEvents.Where(x => x.Timestamp < endAggTime && x.EventCode == 1).OrderByDescending(x => x.Timestamp).FirstOrDefault();
                var lastYellow = cycleEvents.Where(x => x.Timestamp > lastGreen.Timestamp && x.EventCode == 8).OrderBy(x => x.Timestamp).FirstOrDefault();
                if (lastGreen is null || lastYellow is null)
                {
                    continue; //skip this agg and go to the next if there is no timeStamp at all or if there isw no timeStamp in the agg period
                }

                //get the event lists for the agg bin

                var aggDetections = detectorEvents
                    .Where(x => x.Timestamp >= firstGreen.Timestamp &&
                                x.Timestamp <= lastYellow.Timestamp)
                    .OrderBy(x => x.Timestamp);
                var greenList = cycleEvents
                        .Where(x => x.EventCode == 1 &&
                                    x.Timestamp >= firstGreen.Timestamp &&
                                    x.Timestamp <= lastGreen.Timestamp)
                        .OrderBy(x => x.Timestamp);
                var yellowList = cycleEvents
                    .Where(x => x.EventCode == 8 &&
                                x.Timestamp >= firstGreen.Timestamp &&
                                x.Timestamp <= lastYellow.Timestamp)
                    .OrderBy(x => x.Timestamp);
                if (isPermissivePhase && checkAgainstEvents != null)
                {
                    var yProtectedEvents = checkAgainstEvents.Where(x => x.EventCode == 8);
                    var gProtectedEvents = checkAgainstEvents.Where(x => x.EventCode == 1);
                    greenList = (IOrderedEnumerable<IndianaEvent>)CheckProtectedGreens(greenList, yellowList, gProtectedEvents, yProtectedEvents); //redefine the greenList after editng the timeStamp values to start at the beginning of the protected yellow phases if there is overlapping timeStamp time between the protected nad permissive phases (only happens with doghouses)
                }

                //pair each timeStamp with a yellow
                foreach (var timeStamp in greenList.Select(g => g.Timestamp))
                {
                    //Find the corresponding yellow
                    var yellow = yellowList.Where(x => x.Timestamp >= timeStamp).OrderBy(x => x.Timestamp)
                        .FirstOrDefault();
                    if (yellow == null)
                        continue;

                    //get the timeStamp duration
                    TimeSpan greenDuration = yellow.Timestamp - timeStamp;
                    greenDurationList.Add(greenDuration.TotalSeconds);  //if zero, maybe don't add it?  

                    //count the number of cycles
                    cycleCount++;

                    //Find all events between the timeStamp and yellow
                    var greenDetectionsList = aggDetections
                        .Where(x => x.Timestamp >= timeStamp && x.Timestamp <= yellow.Timestamp)
                        .OrderBy(x => x.Timestamp).ToList();
                    if (!greenDetectionsList.Any())
                        continue;
                    //maybe add a catch here where you make sure the BinValue list has as many indexes as it would need to cover the whole timeStamp (and add more if not)
                    //****or maybe add a catch inside the foreach loop below that checks if the intended index exists - and add enough to make sure it does ****

                    //add 1 to the bin value for each detection occuring during timeStamp
                    foreach (var detection in greenDetectionsList)
                    {
                        TimeSpan timeSinceGreenStart = detection.Timestamp - timeStamp;
                        var yAxisBinNumber = (int)(timeSinceGreenStart.TotalSeconds / options.YAxisBinSize);
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
                averageSplits.Add(new DataPointForDouble(StartBinTime, greenDurationList.Average()));
            }

            //get plans
            var plans = planService.GetSplitMonitorPlans(options.Start, options.End, options.LocationIdentifier, planEvents);
            var durYellowRed = GetYellowRedTimeSeconds(options, phaseDetail.PhaseNumber, cycleEvents);
            var programmedSplits = new List<ProgrammedSplit>();
            foreach (Plan analysisplan in plans)
            {
                var splitTime = GetProgrammedSplitTime(phaseDetail.PhaseNumber, analysisplan.Start, analysisplan.End.AddMinutes(-1), controllerEventLogs);
                programmedSplits.Add(new ProgrammedSplit(analysisplan, options.Start, splitTime, durYellowRed));
            }

            GreenTimeUtilizationResult result = new GreenTimeUtilizationResult(
                phaseDetail.Approach.Id,
                phaseDetail.Approach.Location.LocationIdentifier,
                options.Start,
                options.End,
                bins,
                averageSplits,
                programmedSplits.Select(p => new DataPointForDouble(p.Timestamp, p.ProgValue)).ToList(),
                phaseDetail.PhaseNumber,
                options.YAxisBinSize,
                options.XAxisBinSize,
                plans.ToList()
                );
            result.ApproachDescription = phaseDetail.Approach.Description;
            result.LocationDescription = phaseDetail.Approach.Location.LocationDescription();
            return result;
        }

        private static void createStack(List<BarStack> bins, List<int> binValueList, int cycleCount, int xAxisBinNumber)
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
            List<IndianaEvent> controllerEventLogs)
        {
            var eventCode = GetEventCodeForPhase(phaseNumber);
            if (eventCode == null)
                return 0;
            return controllerEventLogs.GetEventsByEventCodes(startDate, endDate, new List<short> { eventCode.Value })
                .Where(e => e.Timestamp <= startDate)
                .OrderByDescending(e => e.Timestamp)
                .Select(e => e.EventParam)
                .FirstOrDefault();
        }

        private static short? GetEventCodeForPhase(int PhaseNumber)
        {
            switch (PhaseNumber)
            {
                case 1:
                    return 134;
                case 2:
                    return 135;
                case 3:
                    return 137;
                case 4:
                    return 138;
                case 5:
                    return 139;
                case 6:
                    return 140;
                case 7:
                    return 141;
                case 8:
                    return 142;
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
                    return null;
            }
        }

        private double GetYellowRedTimeSeconds(
            GreenTimeUtilizationOptions options,
            int phaseNumber,
            List<IndianaEvent> cycleEvents)
        {
            var yrEventNumbers = new List<short> { 8, 11 };
            var yrEvents = cycleEvents.GetEventsByEventCodes(options.Start, options.End, yrEventNumbers, phaseNumber);
            var yellowList = yrEvents.Where(x => x.EventCode == 8)
                .OrderBy(x => x.Timestamp);
            var redList = yrEvents.Where(x => x.EventCode == 11)
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
        IEnumerable<IndianaEvent> CheckProtectedGreens(IEnumerable<IndianaEvent> gPermissiveEvents, IEnumerable<IndianaEvent> yPermissiveEvents, IEnumerable<IndianaEvent> gProtectedEvents, IEnumerable<IndianaEvent> yProtectedEvents)
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
                var yPermissive = yPermissiveEvents
                    .Where(x => x.Timestamp > gPermissive.Timestamp)
                    .OrderBy(x => x.Timestamp)
                    .FirstOrDefault();
                if (yPermissive == null)
                {
                    continue;
                }
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

