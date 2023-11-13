using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Google.Cloud.Logging.Type;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;
using Xunit.Abstractions;
using static System.Net.Mime.MediaTypeNames;

namespace ApplicationCoreTests.Analysis
{
    public class TestingTesting
    {
        private readonly ITestOutputHelper _output;

        public TestingTesting(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async void TestingStuff()
        {
            var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\7115-ApproachDelay.json").FullName);
            var data = JsonConvert.DeserializeObject<TestData>(json);

            _output.WriteLine($"data count: {data.Logs.Count}");

            var testFilteredDetectorData = new TestFilteredDetectorData();
            var testIdentifyandAdjustVehicleActivations = new TestIdentifyandAdjustVehicleActivations();

            var result = new ActionBlock<IReadOnlyList<IGrouping<Detector, IEnumerable<CorrectedDetectorEvent>>>>(a =>
            {
                foreach (var r in a)
                {
                    foreach (var h in r)
                    {
                        _output.WriteLine($"stuff: {r.Key} --- {h.Count()}");
                    }
                }

                //foreach (var r in a.SelectMany(s => s))
                //{
                //    _output.WriteLine($"stuff: {r.Count()}");
                //}


                //_output.WriteLine($"this is a thing: {a}");
            });

            testFilteredDetectorData.LinkTo(testIdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            testIdentifyandAdjustVehicleActivations.LinkTo(result, new DataflowLinkOptions() { PropagateCompletion = true });

            foreach (var a in data.Signal.Approaches)
            {
                testFilteredDetectorData.Post(Tuple.Create<Approach, IEnumerable<ControllerEventLog>>(a, data.Logs.ToList()));
            }

            testFilteredDetectorData.Complete();

            await result.Completion;

            //var test = await testFilteredDetectorData.ReceiveAsync();
            //_output.WriteLine($"this is a thing: {test} - {testFilteredDetectorData.OutputAvailableAsync().Result}");

            
        }
    }

    public class TestData
    {
        public Signal Signal { get; set; }
        public IReadOnlyList<ControllerEventLog> Logs { get; set; }
    }
}
