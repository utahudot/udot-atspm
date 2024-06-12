#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.ApproachSpeed/SpeedDetector.cs
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
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.ApproachSpeed
{
    public class SpeedDetector
    {

        public SpeedDetector(
            List<SpeedPlan> plans,
            int totalDetectorHits,
            DateTime startDate,
            DateTime endDate,
            List<CycleSpeed> cycles,
            List<SpeedEvent> speedEvents,
            AvgSpeedBucketCollection avgSpeedBucketCollection)
        {
            Plans = plans;
            TotalDetectorHits = totalDetectorHits;
            StartDate = startDate;
            EndDate = endDate;
            Cycles = cycles;
            SpeedEvents = speedEvents;
            AvgSpeedBucketCollection = avgSpeedBucketCollection;
        }

        public List<SpeedPlan> Plans { get; set; }
        public int TotalDetectorHits { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CycleSpeed> Cycles { get; set; }
        public List<SpeedEvent> SpeedEvents { get; set; }
        public AvgSpeedBucketCollection AvgSpeedBucketCollection { get; set; }
    }
}