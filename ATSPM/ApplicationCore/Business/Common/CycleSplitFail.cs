#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/CycleSplitFail.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Application.Business.SplitFail;

namespace ATSPM.Application.Business.Common
{
    public class CycleSplitFail : GreenToGreenCycle
    {
        public enum TerminationType
        {
            MaxOut,
            GapOut,
            ForceOff,
            Unknown
        }

        public readonly int FirstSecondsOfRed;

        public CycleSplitFail(DateTime firstGreenEvent, DateTime redEvent, DateTime yellowEvent,
            DateTime lastGreenEvent, TerminationType terminationType,
            int firstSecondsOfRed) : base(firstGreenEvent, yellowEvent, redEvent, lastGreenEvent)
        {
            FirstSecondsOfRed = firstSecondsOfRed;
            TerminationEvent = terminationType;
        }

        public TerminationType TerminationEvent { get; }
        public double RedOccupancyTimeInMilliseconds { get; private set; }
        public double GreenOccupancyTimeInMilliseconds { get; private set; }
        public double GreenOccupancyPercent { get; private set; }
        public double RedOccupancyPercent { get; private set; }
        public bool IsSplitFail { get; private set; }
        public List<SplitFailDetectorActivation> ActivationsDuringGreen { get; set; }
        public List<SplitFailDetectorActivation> ActivationsDuringRed { get; set; }


        public void SetDetectorActivations(List<SplitFailDetectorActivation> detectorActivations)
        {
            var redPeriodToAnalyze = RedEvent.AddSeconds(FirstSecondsOfRed);

            ActivationsDuringRed = detectorActivations.Where
                //detStart AFTER redStart and Before red+AnalaysTime
                (d => d.DetectorOn >= RedEvent && d.DetectorOn < redPeriodToAnalyze
                || d.DetectorOn >= RedEvent && d.DetectorOff > redPeriodToAnalyze

                      //detOff After redStart and Before red+AnalaysTime
                      || d.DetectorOff >= RedEvent && d.DetectorOff < redPeriodToAnalyze

                      //DetStart BEFORE redStart and detOff after cycleEnd
                      || d.DetectorOn <= RedEvent && d.DetectorOff >= redPeriodToAnalyze).OrderBy(d => d.DetectorOn).ToList();
            if (ActivationsDuringRed.Count == 0)
            {
                RedOccupancyTimeInMilliseconds = CheckForDetectorActivationBiggerThanPeriod(RedEvent, redPeriodToAnalyze, detectorActivations);
            }
            else
            {
                RedOccupancyTimeInMilliseconds = GetOccupancy(RedEvent, redPeriodToAnalyze, ActivationsDuringRed);
            }
            ActivationsDuringGreen = detectorActivations.Where(d =>
                d.DetectorOn >= StartTime && d.DetectorOn < YellowEvent ||
                d.DetectorOff >= StartTime && d.DetectorOff < YellowEvent ||
                d.DetectorOn <= StartTime && d.DetectorOff >= YellowEvent).OrderBy(d => d.DetectorOn).ToList();
            if (ActivationsDuringGreen.Count == 0)
            {
                GreenOccupancyTimeInMilliseconds = CheckForDetectorActivationBiggerThanPeriod(StartTime, YellowEvent, detectorActivations);
            }
            else
            {
                GreenOccupancyTimeInMilliseconds = GetOccupancy(StartTime, YellowEvent, ActivationsDuringGreen);
            }
            double millisecondsOfRedStart = FirstSecondsOfRed * 1000;
            RedOccupancyPercent = RedOccupancyTimeInMilliseconds / millisecondsOfRedStart * 100;
            GreenOccupancyPercent = GreenOccupancyTimeInMilliseconds / TotalGreenTimeMilliseconds * 100;
            IsSplitFail = GreenOccupancyPercent > 79 && RedOccupancyPercent > 79;
        }


        private double CheckForDetectorActivationBiggerThanPeriod(DateTime startTime, DateTime endTime,
            List<SplitFailDetectorActivation> detectorActivations)
        {
            if (detectorActivations.Count(d => d.DetectorOn < startTime && d.DetectorOff > endTime) > 0)
                return (endTime - startTime).TotalMilliseconds;
            return 0;
        }

        private double GetOccupancy(DateTime start, DateTime end,
            List<SplitFailDetectorActivation> cycleDetectorActivations)
        {
            double occupancy = 0;
            foreach (var detectorActivation in cycleDetectorActivations)
                //Starts before period and ends after period
                if (detectorActivation.DetectorOn <= start && detectorActivation.DetectorOff >= end)
                    occupancy += (end - start).TotalMilliseconds;
                //Ends in period
                else if (detectorActivation.DetectorOn <= start && detectorActivation.DetectorOff >= start && detectorActivation.DetectorOff <= end)
                    occupancy += (detectorActivation.DetectorOff - start).TotalMilliseconds;
                //Starts in period ends after period
                else if (detectorActivation.DetectorOn >= start && detectorActivation.DetectorOn <= end && detectorActivation.DetectorOff >= end)
                    occupancy += (end - detectorActivation.DetectorOn).TotalMilliseconds;
                //Starts and ends within period and ends within period
                else if (detectorActivation.DetectorOn >= start && detectorActivation.DetectorOn <= end)
                    occupancy += detectorActivation.DurationInMilliseconds;
            return occupancy;
        }
    }
}