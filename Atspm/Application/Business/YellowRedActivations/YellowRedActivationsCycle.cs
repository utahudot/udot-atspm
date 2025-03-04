#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.YellowRedActivations/YellowRedActivationsCycle.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;

namespace Utah.Udot.Atspm.Business.YellowRedActivations
{
    /// <summary>
    ///     Data that represents a red to red cycle for a Location phase
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
        public YellowRedActivationsCycle(
            DateTime yellowClearance,
            DateTime redClearance,
            DateTime red,
            DateTime redEnd,
            double srlvSeconds,
            IReadOnlyList<IndianaEvent> detectorEvents
            )
        {
            SRLVSeconds = srlvSeconds;
            startTime = yellowClearance;
            yellowClearanceEvent = yellowClearance;
            redClearanceEvent = redClearance;
            redEvent = red;
            redEndEvent = redEnd;
            endTime = redEnd;
            if (detectorEvents == null)
            {
                DetectorActivations = new List<YellowRedActivation>();
            }
            else
            {
                DetectorActivations = detectorEvents
                    .Where(d => d.Timestamp >= yellowClearance && d.Timestamp < redEnd)
                    .Select(d => new YellowRedActivation(yellowClearance, d.Timestamp)).ToList();
            }
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
                //check for -1 within tolerance level
                if (violations.AreEqual(-1))
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
                //check for -1 within tolerance level
                if (yellowOccurrences.AreEqual(-1))
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
                //check for -1 within tolerance level
                if (totalYellowTime.AreEqual(-1))
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
                //check for -1 within tolerance level
                if (violations.AreEqual(-1))
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
                //check for -1 within tolerance level
                if (srlv.AreEqual(-1))
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