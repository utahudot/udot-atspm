using ApplicationCoreTests.Analysis.TestObjects;
using ApplicationCoreTests.Attributes;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Workflows;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ApplicationCoreTests.Analysis.Workflows
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
        public void DetectorEventCountAggregationWorkflowTestsFromFile(Location config, List<ControllerEventLog> input, List<DetectorEventCountAggregation> output)
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
                Input = new List<ControllerEventLog>(),
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
