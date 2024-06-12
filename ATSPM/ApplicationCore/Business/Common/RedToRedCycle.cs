#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/RedToRedCycle.cs
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
    /// <summary>
    ///     Data that represents a red to red cycle for a Location phase
    /// </summary>
    public class RedToRedCycle
    {
        public enum EventType
        {
            ChangeToRed,
            ChangeToGreen,
            ChangeToYellow,
            GreenTermination,
            BeginYellowClearance,
            EndYellowClearance,
            Unknown,
            ChangeToEndMinGreen,
            ChangeToEndOfRedClearance,
            OverLapDark
        }

        public RedToRedCycle(DateTime firstRedEvent, DateTime greenEvent, DateTime yellowEvent, DateTime lastRedEvent)
        {
            StartTime = firstRedEvent;
            GreenEvent = greenEvent;
            GreenLineY = (greenEvent - StartTime).TotalSeconds;
            YellowEvent = yellowEvent;
            YellowLineY = (yellowEvent - StartTime).TotalSeconds;
            EndTime = lastRedEvent;
            RedLineY = (lastRedEvent - StartTime).TotalSeconds;
        }

        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public double GreenLineY { get; }
        public double YellowLineY { get; }

        public double RedLineY { get; }
        public DateTime GreenEvent { get; }

        public DateTime YellowEvent { get; }

        public double TotalGreenTimeSeconds => (YellowEvent - GreenEvent).TotalSeconds;
        public double TotalYellowTimeSeconds => (EndTime - YellowEvent).TotalSeconds;
        public double TotalRedTimeSeconds => (GreenEvent - StartTime).TotalSeconds;
        public double TotalTimeSeconds => (EndTime - StartTime).TotalSeconds;
        public double TotalGreenTimeMilliseconds => (YellowEvent - GreenEvent).TotalMilliseconds;
        public double TotalYellowTimeMilliseconds => (EndTime - YellowEvent).TotalMilliseconds;
        public double TotalRedTimeMilliseconds => (GreenEvent - StartTime).TotalMilliseconds;
        public double TotalTimeMilliseconds => (EndTime - StartTime).TotalMilliseconds;
    }
}