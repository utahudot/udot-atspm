using ApplicationCoreTests.Analysis.TestObjects;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Google.Cloud.Logging.Type;
using Microsoft.Extensions.Options;
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
            var file1 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CycleEvents.csv");

            var logs = File.ReadAllLines(file1.FullName)
                   .Skip(1)
                   .Select(x => x.Split(','))
                   .Select(x => new ControllerEventLog
                   {
                       SignalIdentifier = x[0],
                       Timestamp = DateTime.Parse(x[1]),
                       EventCode = int.Parse(x[2]),
                       EventParam = int.Parse(x[3])
                   }).ToList();

            //var file2 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Correction.csv");

            //var events = File.ReadAllLines(file2.FullName)
            //       .Skip(1)
            //       .Select(x => x.Split(','))
            //       .Select(x => new CorrectedDetectorEvent
            //       {
            //           SignalIdentifier = x[0],
            //           CorrectedTimeStamp = DateTime.Parse(x[1]),
            //           //EventCode = int.Parse(x[2]),
            //           DetectorChannel = int.Parse(x[3])
            //       }).ToList();

            //_output.WriteLine($"log count: {logs.Count}");
            //_output.WriteLine($"event count: {events.Count} - {events.Any(a => a.DetectorChannel != 2)}");

            var testdata = new RedToRedCyclesTestData()
            {
                EventLogs = logs,
                //RedCycles = events
            };

            var json = JsonConvert.SerializeObject(testdata);

            File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\RedToRedCyclesTestData.json", json);















            //var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\7115-ApproachDelay.json").FullName);
            //var data = JsonConvert.DeserializeObject<TestData>(json);

            //_output.WriteLine($"data count: {data.Logs.Count}");

            //var testFilteredDetectorData = new FilteredDetectorData();
            //var testIdentifyandAdjustVehicleActivations = new IdentifyandAdjustVehicleActivations();

            //var result = new ActionBlock<IReadOnlyList<Tuple<Detector, IEnumerable<CorrectedDetectorEvent>>>>(a =>
            //{
            //    foreach (var r in a)
            //    {
            //        _output.WriteLine($"stuff: {r.Item1} --- {r.Item2.Count()}");
            //    }
            //});

            //testFilteredDetectorData.LinkTo(testIdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            //testIdentifyandAdjustVehicleActivations.LinkTo(result, new DataflowLinkOptions() { PropagateCompletion = true });

            //foreach (var a in data.Signal.Approaches)
            //{
            //    testFilteredDetectorData.Post(Tuple.Create<Approach, IEnumerable<ControllerEventLog>>(a, data.Logs.ToList()));
            //}

            //testFilteredDetectorData.Complete();

            //await result.Completion;


        }
    }

    public class TestData
    {
        public Signal Signal { get; set; }
        public IReadOnlyList<ControllerEventLog> Logs { get; set; }
    }

    
}
