﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - ApplicationCoreTests.Analysis.Workflows/DetectorEventCountAggregationWorkflowTests.cs
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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utah.Udot.Atspm.Analysis.Workflows;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Attributes;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows
{
    public class DetectorEventCountAggregationWorkflowTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public DetectorEventCountAggregationWorkflowTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [AnalysisTestData<DetectorEventCountAggregationTestData>]
        [Trait(nameof(DetectorEventCountAggregationWorkflow), "From File")]
        //public void DetectorEventCountAggregationWorkflowTestsFromFile(object stuff)
        public void DetectorEventCountAggregationWorkflowTestsFromFile(Location config, List<IndianaEvent> input, List<DetectorEventCountAggregation> output)
        {
            _output.WriteLine($"{config} - {input} - {output}");
        }

        [Fact(Skip = "only run when you need to create test data")]
        public void CreateTestFile()
        {
            var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Location7115TestData.json").FullName);
            var Location = JsonConvert.DeserializeObject<Location>(json);

            var test = new DetectorEventCountAggregationTestData()
            {
                Configuration = Location,
                Input = new List<IndianaEvent>(),
                Output = new List<DetectorEventCountAggregation>()
            };

            var result = JsonConvert.SerializeObject(test, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
            File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\DetectorEventCountAggregationTestData1.json", result);
        }

        [Fact(Skip = "only run when you need to create test data")]
        public void GetTestFile()
        {
            var dir = new DirectoryInfo(Path.Combine(Path.GetFullPath(@"..\..\..\"), "Analysis", "TestData"));

            //var testMethod = System.Reflection.MethodInfo.GetCurrentMethod();

            if (dir.Exists)
            {
                foreach (var f in dir.GetFiles("*.json").Where(f => f.Name.Contains(typeof(DetectorEventCountAggregationTestData).Name)))
                {
                    _output.WriteLine($"{f.FullName}");

                    var json = File.ReadAllText(f.FullName);
                    var testFile = JsonConvert.DeserializeObject<DetectorEventCountAggregationTestData>(json, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

                    _output.WriteLine($"{testFile.Configuration}");
                }
            }
        }


        public void Dispose()
        {
        }
    }
}
