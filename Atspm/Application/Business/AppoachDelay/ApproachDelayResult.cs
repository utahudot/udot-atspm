#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.AppoachDelay/ApproachDelayResult.cs
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

using System.Text.Json;
using Utah.Udot.Atspm.Business.Common;

namespace Utah.Udot.Atspm.Business.AppoachDelay
{
    public class ApproachDelayResult : ApproachResult
    {
        public ApproachDelayResult()
        {

        }

        public ApproachDelayResult(
            int approachId,
            string locationId,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            double averageDelayPerVehicle,
            double totalDelay,
            List<ApproachDelayPlan> plans,
            List<DataPointForDouble> approachDelayDataPoints,
            List<DataPointForDouble> approachDelayPerVehicleDataPoints) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            AverageDelayPerVehicle = averageDelayPerVehicle;
            TotalDelay = totalDelay;
            Plans = plans;
            ApproachDelayDataPoints = approachDelayDataPoints;
            ApproachDelayPerVehicleDataPoints = approachDelayPerVehicleDataPoints;
        }

        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public double AverageDelayPerVehicle { get; set; }
        public double TotalDelay { get; set; }


        public List<ApproachDelayPlan> Plans { get; set; }


        public List<DataPointForDouble> ApproachDelayDataPoints { get; set; }


        public List<DataPointForDouble> ApproachDelayPerVehicleDataPoints { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

}
