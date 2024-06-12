#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/GreenToGreenCycle.cs
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
    public class GreenToGreenCycle
    {
        public enum EventType
        {
            ChangeToRed,
            ChangeToGreen,
            ChangeToYellow,
            GreenTermination,
            BeginYellowClearance,
            EndYellowClearance,
            Unknown
        }

        public GreenToGreenCycle(DateTime firstGreenEvent, DateTime yellowEvent, DateTime redEvent,
            DateTime lastGreenEvent)
        {
            StartTime = firstGreenEvent;
            RedEvent = redEvent;
            RedLineY = (RedEvent - StartTime).TotalSeconds;
            YellowEvent = yellowEvent;
            YellowLineY = (yellowEvent - StartTime).TotalSeconds;
            EndTime = lastGreenEvent;
            GreenLineY = (lastGreenEvent - StartTime).TotalSeconds;
            //PreemptCollection = new List<DetectorDataPoint>();
        }

        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public double GreenLineY { get; }
        public double YellowLineY { get; }

        public double RedLineY { get; }

        //public List<DetectorDataPoint> PreemptCollection { get; }
        public DateTime RedEvent { get; }

        public DateTime YellowEvent { get; }


        public double TotalGreenTime => (YellowEvent - StartTime).TotalSeconds;
        public double TotalYellowTime => (RedEvent - YellowEvent).TotalSeconds;
        public double TotalRedTime => (EndTime - RedEvent).TotalSeconds;
        public double TotalTime => (EndTime - StartTime).TotalSeconds;
        public double TotalGreenTimeMilliseconds => (YellowEvent - StartTime).TotalMilliseconds;
        public double TotalYellowTimeMilliseconds => (RedEvent - YellowEvent).TotalMilliseconds;
        public double TotalRedTimeMilliseconds => (EndTime - RedEvent).TotalMilliseconds;
        public double TotalTimeMilliseconds => (EndTime - StartTime).TotalMilliseconds;
    }
}