using ApplicationCoreTests.Analysis.TestObjects;
using ATSPM.Application;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Common;
using ATSPM.Application.Extensions;
using ATSPM.Application.Specifications;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using AutoFixture;
using Google.Cloud.Logging.Type;
using Microsoft.Extensions.Options;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.Operation.Buffer;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;
using Xunit.Abstractions;
using static System.Net.Mime.MediaTypeNames;

namespace ApplicationCoreTests.Analysis
{
    //public class TempPhaseDirections
    //{
    //    private readonly IEnumerable<ControllerEventLog> _logs;

    //    public TempPhaseDirections(IEnumerable<ControllerEventLog> logs)
    //    {
    //        _logs = logs;
    //    }

    //    public Approach Primary { get; set; }
    //    public Approach Opposing { get; set; }

    //    public IReadOnlyList<ControllerEventLog> PrimaryLogs => _logs.Where(w => w.SignalIdentifier == Primary.Signal.SignalIdentifier && Primary.Detectors.Select(s => s.DetectorChannel).Contains(w.EventParam)).ToList();
    //    public IReadOnlyList<ControllerEventLog> OpposingLogs => _logs.Where(w => w.SignalIdentifier == Opposing.Signal.SignalIdentifier && Opposing.Detectors.Select(s => s.DetectorChannel).Contains(w.EventParam)).ToList();
    //}

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
            //var file1 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\ApproachVolumeTestLogs.csv");

            //var logs = File.ReadAllLines(file1.FullName)
            //       .Skip(1)
            //       .Select(x => x.Split(','))
            //       .Select(x => new ControllerEventLog
            //       {
            //           SignalIdentifier = x[0],
            //           Timestamp = DateTime.Parse(x[1]),
            //           EventCode = int.Parse(x[2]),
            //           EventParam = int.Parse(x[3])
            //       }).ToList();

            ////var testdata = new RedToRedCyclesTestData()
            ////{
            ////    EventLogs = logs,
            ////    //RedCycles = events
            ////};

            //var json = JsonConvert.SerializeObject(logs);

            //File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\ApproachVolumeTestLogs.json", json);

            //var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Signal7115TestData.json").FullName);
            //var signal = JsonConvert.DeserializeObject<Signal>(json);

            //var json1 = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculatePhaseVolumeTestData1.json").FullName);
            //var json2 = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculatePhaseVolumeTestData2.json").FullName);

            //var data1 = JsonConvert.DeserializeObject<CalculatePhaseVolumeTestData>(json1);
            //var data2 = JsonConvert.DeserializeObject<CalculatePhaseVolumeTestData>(json2);

            ////signal.Approaches.Clear();
            ////signal.Approaches.Add(data1.Configuration);
            ////signal.Approaches.Add(data2.Configuration);

            //var c = new CalculateTotalVolumes();



            //var t1 = Tuple.Create(data1.Configuration, data1.Output);
            //var t2 = Tuple.Create(data2.Configuration, data2.Output);

            //var tv = await c.ExecuteAsync(Tuple.Create(t1, t2));

            //var result = new CalculateTotalVolumeTestData()
            //{
            //    Configuration = new List<Approach>() { data1.Configuration, data2.Configuration },
            //    Input = new List<Volumes>() { data1.Output, data2.Output },
            //    Output = tv.Item2
            //};


            //var test = JsonConvert.SerializeObject(result);
            //File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculateTotalVolumesTestData1.json", test);





            //var times = Enumerable.Range(1, 10).Select(s => DateTime.Now.AddMinutes(s)).ToList();

            var tl = new Timeline<StartEndRange>(DateTime.Now, DateTime.Now.AddHours(2), TimeSpan.FromMinutes(15));

            var test = tl.Segments.Select(a => a.End - a.Start).Average(a => a.TotalSeconds);
            var ts = TimeSpan.FromSeconds(test);

            _output.WriteLine($"test {test}");
            _output.WriteLine($"ts {ts}");



        }
    }

    public class TestData
    {
        public Signal Signal { get; set; }
        public IReadOnlyList<ControllerEventLog> Logs { get; set; }
    }


}
