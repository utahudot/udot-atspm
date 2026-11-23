#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects/IdentifyandAdjustVehicleActivationsTestData.cs
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

using System.Collections.Generic;
using Utah.Udot.Atspm.Analysis.Common;
using Utah.Udot.Atspm.Analysis.PreemptionDetails;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects
{
    /// <summary>
    /// Base container for external workflow and workflow-step test data consisting of
    /// configuration, raw or intermediate input data, and expected output results.
    /// </summary>
    public abstract class AnalysisTestDataBase
    {
        /// <summary>
        /// Gets or sets the configuration object for the test scenario.
        /// </summary>
        public object Configuration { get; set; }

        /// <summary>
        /// Gets or sets the input payload for the workflow step.
        /// </summary>
        public object Input { get; set; }

        /// <summary>
        /// Gets or sets the expected output from the workflow step.
        /// </summary>
        public object Output { get; set; }
    }

    /// <summary>
    /// Strongly-typed generic container for external workflow and workflow-step test data.
    /// Enables clean JSON serialization and deserialization without requiring <c>$type</c> metadata on every property.
    /// </summary>
    /// <typeparam name="TConfig">The configuration type (e.g., <see cref="Location"/> or existing <see cref="SignalTimingPlan"/> list).</typeparam>
    /// <typeparam name="TInput">The input type passed to the step.</typeparam>
    /// <typeparam name="TOutput">The expected output type returned by the step.</typeparam>
    public abstract class AnalysisTestDataBase<TConfig, TInput, TOutput> : AnalysisTestDataBase
    {
        /// <summary>
        /// Gets or sets the strongly-typed configuration for the test scenario.
        /// </summary>
        public new TConfig Configuration
        {
            get => base.Configuration is TConfig config ? config : default;
            set => base.Configuration = value;
        }

        /// <summary>
        /// Gets or sets the strongly-typed input payload for the workflow step.
        /// </summary>
        public new TInput Input
        {
            get => base.Input is TInput input ? input : default;
            set => base.Input = value;
        }

        /// <summary>
        /// Gets or sets the strongly-typed expected output from the workflow step.
        /// </summary>
        public new TOutput Output
        {
            get => base.Output is TOutput output ? output : default;
            set => base.Output = value;
        }
    }

    /// <summary>
    /// Test data class for detector event count aggregation tests.
    /// </summary>
    public class AggregateDetectorEventCountTestData : AnalysisTestDataBase { }

    /// <summary>
    /// Test data class for signal event count aggregation tests.
    /// </summary>
    public class AggregateSignalEventCountTestData : AnalysisTestDataBase { }

    /// <summary>
    /// Test data class for pedestrian phase aggregation tests.
    /// </summary>
    public class AggregatePedestrianPhasesTestData : AnalysisTestDataBase { }

    /// <summary>
    /// Test data class for phase cycle aggregation tests.
    /// </summary>
    public class AggregatePhaseCycleTestData : AnalysisTestDataBase { }

    /// <summary>
    /// Test data class for phase split monitor aggregation tests.
    /// </summary>
    public class AggregatePhaseSplitMonitorData : AnalysisTestDataBase { }

    /// <summary>
    /// Test data class for preemption aggregation tests.
    /// </summary>
    public class AggregatePreemptTestData : AnalysisTestDataBase { }

    /// <summary>
    /// Test data class for transit signal priority aggregation tests.
    /// </summary>
    public class AggregatePriorityTestData : AnalysisTestDataBase { }

    /// <summary>
    /// Strongly-typed test data class for <c>GenerateSignalPlansStep</c> unit tests.
    /// </summary>
    public class GenerateSignalPlansTestData : AnalysisTestDataBase<Location, List<IndianaEvent>, List<SignalTimingPlan>> { }

    /// <summary>
    /// Strongly-typed test data class for <c>SaveSignalTimingPlansStep</c> unit tests.
    /// </summary>
    public class SaveSignalTimingPlansTestData : AnalysisTestDataBase<Location, List<SignalTimingPlan>, List<SignalTimingPlan>> { }

    /// <summary>
    /// Test data class for vehicle activation identification and adjustment tests.
    /// </summary>
    public class IdentifyandAdjustVehicleActivationsTestData : AnalysisTestDataBase<Approach, List<IndianaEvent>, List<CorrectedDetectorEvent>> { }

    /// <summary>
    /// Test data class for phase volume calculation tests.
    /// </summary>
    public class CalculatePhaseVolumeTestData : AnalysisTestDataBase<Approach, List<CorrectedDetectorEvent>, Volumes> { }

    /// <summary>
    /// Test data class for total volume calculation tests.
    /// </summary>
    public class CalculateTotalVolumeTestData : AnalysisTestDataBase<List<Approach>, List<Volumes>, TotalVolumes> { }

    /// <summary>
    /// Test data class for red-to-red cycle identification tests.
    /// </summary>
    public class RedToRedCyclesTestData : AnalysisTestDataBase<Approach, List<IndianaEvent>, List<RedToRedCycle>> { }

    /// <summary>
    /// Test data class for preemptive process calculation tests.
    /// </summary>
    public class PreemptiveProcessTestData : AnalysisTestDataBase<Location, List<IndianaEvent>, List<PreempDetailValueBase>> { }
}
