#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/DetectorDataPoint.cs
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
    public enum ArrivalType
    {
        ArrivalOnGreen,
        ArrivalOnYellow,
        ArrivalOnRed
    }

    public class DetectorDataPoint
    {
        public DetectorDataPoint(DateTime startDate, DateTime eventTime, DateTime greenEvent, DateTime yellowEvent)
        {
            TimeStamp = eventTime;
            StartOfCycle = startDate;
            YPointSeconds = (TimeStamp - StartOfCycle).TotalSeconds;
            YellowEvent = yellowEvent;
            GreenEvent = greenEvent;
            SetDataPointProperties();
        }

        private void SetDataPointProperties()
        {
            //if the detector hit is before greenEvent
            if (TimeStamp < GreenEvent)
            {
                var test = GreenEvent - TimeStamp;
                DelaySeconds = (GreenEvent - TimeStamp).TotalSeconds;
                ArrivalType = ArrivalType.ArrivalOnRed;
            }
            //if the detector hit is After green, but before yellow
            else if (TimeStamp >= GreenEvent && TimeStamp < YellowEvent)
            {
                DelaySeconds = 0;
                ArrivalType = ArrivalType.ArrivalOnGreen;
            }
            //if the event time is after yellow
            else if (TimeStamp >= YellowEvent)
            {
                DelaySeconds = 0;
                ArrivalType = ArrivalType.ArrivalOnYellow;
            }
        }

        //Represents a time span from the start of the red to red cycle
        public double YPointSeconds { get; private set; }

        public DateTime StartOfCycle { get; }

        //The actual time of the detector activation
        public DateTime TimeStamp { get; private set; }

        public DateTime YellowEvent { get; }

        public DateTime GreenEvent { get; }

        public double DelaySeconds { get; set; }

        public ArrivalType ArrivalType { get; set; }

        public void AddSeconds(int seconds)
        {
            TimeStamp = TimeStamp.AddSeconds(seconds);
            YPointSeconds = (TimeStamp - StartOfCycle).TotalSeconds;
            SetDataPointProperties();
        }
    }
}