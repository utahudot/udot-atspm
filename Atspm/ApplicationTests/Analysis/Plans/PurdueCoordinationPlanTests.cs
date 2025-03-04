#region license
// Copyright 2025 Utah Departement of Transportation
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

using Utah.Udot.Atspm.Analysis.Common;
using Utah.Udot.Atspm.Analysis.Plans;
using Utah.Udot.Atspm.Analysis.PurdueCoordination;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.Enums;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.NetStandardToolkit.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

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
        public async void Test()
        {
            //var sut = new GeneratePurdueCoordinationResult();

            //var testEvents = Enumerable.Range(1, 10).Select(s => new IndianaEvent()
            //{
            //    locationId = "1001",
            //    EventCode = (int)DataLoggerEnum.CoordPatternChange,
            //    EventParam = 1,
            //    Timestamp = DateTime.Now.AddSeconds(s)

            //});

            //var result = await sut.ExecuteAsync(testEvents);

            //var condition = result.SelectMany(s => s).All(a => Math.Round((a.End - a.Start).TotalSeconds, 0) == 1);

            Assert.False(true);
        }



        public void Dispose()
        {
        }
    }
}
