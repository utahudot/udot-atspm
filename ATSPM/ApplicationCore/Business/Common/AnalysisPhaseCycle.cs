#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/AnalysisPhaseCycle.cs
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

namespace ATSPM.Application.Business.Common
{
    public class AnalysisPhaseCycle
    {
        public enum NextEventResponse
        {
            CycleOK,
            CycleMissingData,
            CycleComplete
        }

        public enum TerminationType
        {
            GapOut = 4,
            MaxOut = 5,
            ForceOff = 6,
            Unknown = 0
        }

        private double pedDuration;

        /// <summary>
        ///     Phase Objects primarily for the split monitor and terminaiton chart
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="phasenumber"></param>
        /// <param name="starttime"></param>
        public AnalysisPhaseCycle(string locationId, int phasenumber, DateTime starttime)
        {
            locationId = locationId;
            PhaseNumber = phasenumber;
            StartTime = starttime;
            HasPed = false;
            TerminationEvent = null;
        }

        public int PhaseNumber { get; }

        public string locationId { get; }

        public DateTime StartTime { get; }

        public DateTime EndTime { get; private set; }

        public DateTime PedStartTime { get; private set; }

        public DateTime PedEndTime { get; private set; }

        public short? TerminationEvent { get; private set; }

        public TimeSpan Duration { get; private set; }

        public double PedDuration
        {
            get
            {
                if (pedDuration > 0)
                    return pedDuration;
                return 0;
            }
        }


        public bool HasPed { get; set; }

        public DateTime YellowEvent { get; set; }

        public void SetTerminationEvent(short terminatonCode)
        {
            TerminationEvent = terminatonCode;
        }

        public void SetEndTime(DateTime endtime)
        {
            EndTime = endtime;
            Duration = EndTime.Subtract(StartTime);
        }

        public void SetPedStart(DateTime starttime)
        {
            PedStartTime = starttime;
            HasPed = true;
        }

        public void SetPedEnd(DateTime endtime)
        {
            PedEndTime = endtime;
            pedDuration = PedEndTime.Subtract(PedStartTime).TotalSeconds;
        }
    }
}