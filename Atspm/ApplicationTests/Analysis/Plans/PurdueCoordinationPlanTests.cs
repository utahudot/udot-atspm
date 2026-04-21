#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - %Namespace%/PurdueCoordinationPlanTests.cs
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
using System.IO;
using System.Linq;
using Utah.Udot.Atspm.Analysis.Common;
using Utah.Udot.Atspm.Analysis.Plans;
using Utah.Udot.Atspm.Analysis.PurdueCoordination;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Enums;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Common;
using Utah.Udot.NetStandardToolkit.Extensions;
using Xunit;
using Xunit.Abstractions;
using Utah.Udot.Atspm.Extensions;
using System.Threading.Tasks;

namespace ApplicationCoreTests.Analysis.Plans
{
    public class PurdueCoordinationPlanTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public PurdueCoordinationPlanTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait(nameof(PurdueCoordinationPlanTests), "DurationCheck")]
        public void Test()
        {
            var testData = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 11:10:01.1"), EventCode = 131, EventParam = 5},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 11:20:01.1"), EventCode = 54, EventParam = 1},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 11:30:01.1"), EventCode = 131, EventParam = 1},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 11:40:01.1"), EventCode = 131, EventParam = 1},
                new IndianaEvent() { LocationIdentifier = "2000", Timestamp = DateTime.Parse("4/17/2023 12:10:01.1"), EventCode = 131, EventParam = 5},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:20:01.1"), EventCode = 54, EventParam = 1},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:30:01.1"), EventCode = 131, EventParam = 2},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:40:01.1"), EventCode = 131, EventParam = 2},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:50:01.1"), EventCode = 131, EventParam = 27},
            };

            var planIntervals = testData
                .FromSpecification(new IndianaPlanDataSpecification())
                .GroupBy(e => e.LocationIdentifier)
                .SelectMany(group =>
                {
                     var unique = group.KeepFirstSequentialParam().ToList();

                     if (unique.Count == 0) return Enumerable.Empty<SignalPlanAggregation>();

                     return unique.Zip(unique.Skip(1).Append(null), (current, next) => new SignalPlanAggregation
                     {
                         LocationIdentifier = current.LocationIdentifier,
                         PlanNumber = current.EventParam,
                         Start = current.Timestamp,
                         End = next?.Timestamp ?? DateTime.MinValue
                     });
                 })
                 .ToList();

            foreach (var p in planIntervals)
            {
                _output.WriteLine($"Location: {p.LocationIdentifier}, Plan: {p.PlanNumber}, Start: {p.Start}, End: {p.End}");
            }

        }

        public void Dispose()
        {
        }
    }
}
