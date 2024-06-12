#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.TimeSpaceDiagram/TimeSpaceService.cs
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
using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.TimingAndActuation;
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.TimeSpaceDiagram
{
    public static class TimeSpaceService
    {
        private static readonly double FeetPerMile = 5280;
        private static readonly double SecondsInHour = 3600;

        public static List<TimeSpaceEventBase> GetGreenTimeEvents(
            List<CycleEventsDto> cycleEvents,
            int speedLimit,
            double distanceToNextLocation)
        {
            List<int> cycleGreenStartEndCodes = new List<int>() { 1, 8 };
            var events = new List<CycleEventsDto>();
            var greenTimeEvents = new List<TimeSpaceEventBase>();
            var tempEvents = cycleEvents.Where(c => cycleGreenStartEndCodes.Contains(c.Value)).ToList();

            foreach (var gEvent in tempEvents)
            {
                DateTime start = gEvent.Start;
                GetArrivalTime(distanceToNextLocation, speedLimit, start, out _, out DateTime arrivalTime);
                TimeSpaceEventBase resultOn = new TimeSpaceEventBase(
                    start,
                    arrivalTime,
                    gEvent.Value == 1 ? true : false);
                greenTimeEvents.Add(resultOn);
            }
            return greenTimeEvents;
        }

        public static void GetArrivalTime(double distanceToDetector, double speedLimit, DateTime InitialTime, out double speedInFeetPerSecond, out DateTime arrivalTime)
        {
            DateTime currentDetectorOn = InitialTime;

            speedInFeetPerSecond = GetSpeedInFeetPerSecond(speedLimit);
            double timeToTravel = distanceToDetector / speedInFeetPerSecond;

            arrivalTime = currentDetectorOn.AddSeconds(timeToTravel);
        }

        private static double GetSpeedInFeetPerSecond(double speedLimit)
        {
            return speedLimit * FeetPerMile / SecondsInHour;
        }

        public static List<IndianaEvent> GetEvents(int phaseNumber, List<IndianaEvent> controllerEventLogs, List<short> cycleEventCodes)
        {
            var distinctTimeStamps = new HashSet<string>();
            var tempEvents = controllerEventLogs.Aggregate(new List<IndianaEvent>(), (result, c) =>
            {
                if (cycleEventCodes.Contains(c.EventCode) && c.EventParam == phaseNumber)
                {
                    if (!distinctTimeStamps.Contains(c.ToString()))
                    {
                        result.Add(c);
                        distinctTimeStamps.Add(c.ToString());
                    }
                }
                return result;
            });
            return tempEvents;
        }

        public static List<short> GetCycleCodes(bool getOverlapCodes)
        {
            var phaseEventCodesForCycles = new List<short> { 1, 8, 9 };
            if (getOverlapCodes)
            {
                phaseEventCodesForCycles = new List<short> {
                    61,
                    62,
                    63,
                    64,
                    65 };
            }

            return phaseEventCodesForCycles;
        }

        public static string GetPhaseSort(PhaseDetail phaseDetail)
        {
            return phaseDetail.IsPermissivePhase ?  // Check if the 'GetPermissivePhase' property of 'options' is true
                phaseDetail.Approach.IsPermissivePhaseOverlap ?  // If true, check if the 'IsPermissivePhaseOverlap' property of 'approach' is true
                    "zOverlap - " + phaseDetail.Approach.PermissivePhaseNumber.Value.ToString("D2")  // If true, concatenate "zOverlap - " with 'PermissivePhaseNumber' formatted as a two-digit string
                    : "Phase - " + phaseDetail.Approach.PermissivePhaseNumber.Value.ToString("D2")  // If false, concatenate "Phase - " with 'PermissivePhaseNumber' formatted as a two-digit string
                :  // If 'GetPermissivePhase' is false
                phaseDetail.Approach.IsProtectedPhaseOverlap ?  // Check if the 'IsProtectedPhaseOverlap' property of 'approach' is true
                    "zOverlap - " + phaseDetail.Approach.ProtectedPhaseNumber.ToString("D2")  // If true, concatenate "zOverlap - " with 'ProtectedPhaseNumber' formatted as a two-digit string
                    : "Phase = " + phaseDetail.Approach.ProtectedPhaseNumber.ToString("D2");  // If false, concatenate "Phase = " with 'ProtectedPhaseNumber' formatted as a two-digit string
        }
    }
}
