#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/CyclePcd.cs
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

namespace ATSPM.Application.Business.Common
{
    /// <summary>
    ///     Data that represents a red to red cycle for a Location phase
    /// </summary>
    public class CyclePcd : RedToRedCycle
    {
        public CyclePcd(DateTime firstRedEvent, DateTime greenEvent, DateTime yellowEvent, DateTime lastRedEvent) :
            base(firstRedEvent, greenEvent, yellowEvent, lastRedEvent)
        {
            DetectorEvents = new List<DetectorDataPoint>();
        }

        public List<DetectorDataPoint> DetectorEvents { get; }

        public double TotalArrivalOnGreen => DetectorEvents.Count(d => d.ArrivalType == ArrivalType.ArrivalOnGreen);
        public double TotalArrivalOnYellow => DetectorEvents.Count(d => d.ArrivalType == ArrivalType.ArrivalOnYellow);
        public double TotalArrivalOnRed => DetectorEvents.Count(d => d.ArrivalType == ArrivalType.ArrivalOnRed);
        public double TotalDelaySeconds => DetectorEvents.Sum(d => d.DelaySeconds);
        public double TotalVolume => DetectorEvents.Count(d => d.TimeStamp >= StartTime && d.TimeStamp < EndTime);

        public void AddDetectorData(DetectorDataPoint ddp)
        {
            DetectorEvents.Add(ddp);
        }

        public void AddSecondsToDetectorEvents(int seconds)
        {
            foreach (var detectorDataPoint in DetectorEvents)
            {
                detectorDataPoint.AddSeconds(seconds);
            }
        }
    }
}