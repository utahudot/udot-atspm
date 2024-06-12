#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - ApplicationCoreTests.Analysis.TestObjects/IdentifyandAdjustVehicleActivationsTestData.cs
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
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using System.Collections.Generic;

namespace ApplicationCoreTests.Analysis.TestObjects
{
    public abstract class AnalysisTestDataBase
    {
        public object Configuration { get; set; }
        public object Input { get; set; }
        public object Output { get; set;}
    }

    public class DetectorEventCountAggregationTestData : AnalysisTestDataBase { }









    public class IdentifyandAdjustVehicleActivationsTestData
    {
        public Approach Configuration { get; set; }
        public List<ControllerEventLog> Input { get; set; }
        public List<CorrectedDetectorEvent> Output { get; set; }
    }

    public class CalculatePhaseVolumeTestData
    {
        public Approach Configuration { get; set; }
        public List<CorrectedDetectorEvent> Input { get; set; }
        public Volumes Output { get; set; }
    }

    public class CalculateTotalVolumeTestData
    {
        public List<Approach> Configuration { get; set; }
        public List<Volumes> Input { get; set; }
        public TotalVolumes Output { get; set; }
    }

    public class RedToRedCyclesTestData
    {
        public Approach Configuration { get; set; }
        public List<ControllerEventLog> Input { get; set; }
        public List<RedToRedCycle> Output { get; set; }
    }

    public class PreemptiveProcessTestData
    {
        public Location Configuration { get; set; }
        public List<ControllerEventLog> Input { get; set; }
        public List<PreempDetailValueBase> Output { get; set; }
    }

    public class AggregatePriorityCodesTestData
    {
        public Location Configuration { get; set; }
        public List<ControllerEventLog> Input { get; set; }
        public List<PriorityAggregation> Output { get; set; }
    }

    public class AggregatePreemptCodesTestData
    {
        public Location Configuration { get; set; }
        public List<ControllerEventLog> Input { get; set; }
        public List<PreemptionAggregation> Output { get; set; }
    }
}
