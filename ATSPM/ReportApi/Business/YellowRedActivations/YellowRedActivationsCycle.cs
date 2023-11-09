using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.ReportApi.Business.YellowRedActivations
{
    /// <summary>
    ///     Data that represents a red to red cycle for a signal phase
    /// </summary>
    public class YellowRedActivationsCycle
    {

        /// <summary>
        ///     Start time of the Cycle
        /// </summary>
        protected DateTime endTime;

        /// <summary>
        ///     Y coordinate for the yellow line on the chart
        /// </summary>
        protected double redBeginY;

        /// <summary>
        ///     Y coordinate for the yellow clearance begin line on the chart
        /// </summary>
        protected double redClearanceBeginY;

        /// <summary>
        ///     Green time of the Cycle
        /// </summary>
        protected DateTime redClearanceEvent;

        /// <summary>
        ///     Green time of the Cycle
        /// </summary>
        protected DateTime redEndEvent;

        /// <summary>
        ///     Y coordinate for the red line on the chart
        /// </summary>
        protected double redEndY;

        /// <summary>
        ///     Green time of the Cycle
        /// </summary>
        protected DateTime redEvent;

        private double srlv = -1;

        /// <summary>
        ///     Start time of the cycle
        /// </summary>
        protected DateTime startTime;

        private double totalViolationTime;

        private double totalYellowTime;

        private double violations = -1;

        /// <summary>
        ///     Y coordinate for the yellow clearance begin line on the chart
        /// </summary>
        protected double yellowClearanceBeginY;


        /// <summary>
        ///     Yellow time of the Cycle
        /// </summary>
        protected DateTime yellowClearanceEvent;

        private double yellowOccurrences = -1;


        /// <summary>
        ///     Constructor for the PCDDataPointGroup
        /// </summary>
        /// <param name="cycleStartTime"></param>
        public YellowRedActivationsCycle(
            DateTime yellowClearance,
            DateTime redClearance,
            DateTime red,
            DateTime redEnd,
            double srlvSeconds,
            IReadOnlyList<ControllerEventLog> detectorEvents
            )
        {
            SRLVSeconds = srlvSeconds;
            startTime = yellowClearance;
            yellowClearanceEvent = yellowClearance;
            redClearanceEvent = redClearance;
            redEvent = red;
            redEndEvent = redEnd;
            endTime = redEnd;
            DetectorActivations = detectorEvents
                .Where(d => d.Timestamp >= yellowClearance && d.Timestamp < redEnd)
                .Select(d => new YellowRedActivation(yellowClearance, d.Timestamp)).ToList();
        }

        public DateTime StartTime => startTime;

        public DateTime EndTime => endTime;

        public double YellowClearanceBeginY => yellowClearanceBeginY;

        public double RedClearanceBeginY => redClearanceBeginY;

        public double RedBeginY => redBeginY;

        public double Violations
        {
            get
            {
                if (violations == -1)
                {
                    violations = 0;
                    foreach (var d in DetectorActivations)
                        if (d.TimeStamp > RedClearanceEvent)
                        {
                            violations++;
                            totalViolationTime += (d.TimeStamp - RedClearanceEvent).TotalSeconds;
                        }
                }
                return violations;
            }
        }

        public double YellowOccurrences
        {
            get
            {
                if (yellowOccurrences == -1)
                {
                    yellowOccurrences = 0;
                    foreach (var d in DetectorActivations)
                        if (d.TimeStamp <= RedClearanceEvent)
                        {
                            yellowOccurrences++;
                            totalYellowTime += (d.TimeStamp - RedClearanceEvent).TotalSeconds;
                        }
                }
                return yellowOccurrences;
            }
        }

        public double TotalYellowTime
        {
            get
            {
                //because YellowActivations is lazy loaded make sure it is set which also
                //sets YellowActivations
                if (totalYellowTime == -1)
                {
                    var temp = YellowOccurrences;
                }
                return totalYellowTime;
            }
        }

        public double TotalViolationTime
        {
            get
            {
                //because violations is lazy loaded make sure it is set which also
                //sets totalViolationTime
                if (violations == -1)
                {
                    var temp = Violations;
                }
                return totalViolationTime;
            }
        }

        public double SRLVSeconds { get; }

        public double SevereRedLightViolations
        {
            get
            {
                if (srlv == -1)
                {
                    srlv = 0;
                    foreach (var d in DetectorActivations)
                        if (d.TimeStamp > RedClearanceEvent.AddSeconds(SRLVSeconds))
                            srlv++;
                }
                return srlv;
            }
        }

        /// <summary>
        ///     A collection of detector activations for the cycle
        /// </summary>

        public List<YellowRedActivation> DetectorActivations { get; set; }

        public DateTime YellowClearanceEvent => yellowClearanceEvent;

        public DateTime RedClearanceEvent => redClearanceEvent;

        public DateTime RedEvent => redEvent;
    }
}