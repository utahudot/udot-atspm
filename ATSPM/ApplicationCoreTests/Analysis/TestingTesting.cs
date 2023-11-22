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
using Google.Cloud.Logging.Type;
using Microsoft.Extensions.Options;
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
    public class TempPhaseDirections
    {
        private readonly IEnumerable<ControllerEventLog> _logs;

        public TempPhaseDirections(IEnumerable<ControllerEventLog> logs)
        {
            _logs = logs;
        }

        public Approach Primary { get; set; }
        public Approach Opposing { get; set; }

        public IReadOnlyList<ControllerEventLog> PrimaryLogs => _logs.Where(w => w.SignalIdentifier == Primary.Signal.SignalIdentifier && Primary.Detectors.Select(s => s.DetectorChannel).Contains(w.EventParam)).ToList();
        public IReadOnlyList<ControllerEventLog> OpposingLogs => _logs.Where(w => w.SignalIdentifier == Opposing.Signal.SignalIdentifier && Opposing.Detectors.Select(s => s.DetectorChannel).Contains(w.EventParam)).ToList();
    }

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

            ////var json1 = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Temp.json").FullName);
            //var json1 = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculatePhaseVolumeTestData1.json").FullName);
            //var json2 = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculatePhaseVolumeTestData2.json").FullName);

            //var data1 = JsonConvert.DeserializeObject<CalculatePhaseVolumeTestData>(json1);
            //var data2 = JsonConvert.DeserializeObject<CalculatePhaseVolumeTestData>(json2);



            //signal.Approaches.Clear();
            //signal.Approaches.Add(data1.Configuration);
            //signal.Approaches.Add(data2.Configuration);

            //var c = new CalculateTotalVolumes();

            //_output.WriteLine($"start: {data1.Output.Min(m => m.Start).RoundDown(TimeSpan.FromMinutes(15))}");

            //data1.Output.Start = data1.Output.Min(m => m.Start).RoundDown(TimeSpan.FromMinutes(15));
            //data1.Output.End = data1.Output.Max(m => m.Start).RoundUp(TimeSpan.FromMinutes(15));
            //data2.Output.Start = data2.Output.Min(m => m.Start).RoundDown(TimeSpan.FromMinutes(15));
            //data2.Output.End = data2.Output.Max(m => m.Start).RoundUp(TimeSpan.FromMinutes(15));

            //_output.WriteLine($"Output1: {data1.Output.Start}");
            //_output.WriteLine($"Output1: {data1.Output.End}");
            //_output.WriteLine($"Output2: {data2.Output.Start}");
            //_output.WriteLine($"Output2: {data2.Output.End}");

            //var t1 = Tuple.Create(data1.Configuration, data1.Output);
            //var t2 = Tuple.Create(data2.Configuration, data2.Output);

            //var tv = await c.ExecuteAsync(Tuple.Create(t1, t2));

            //_output.WriteLine($"tv1: {tv.Item1}");
            //_output.WriteLine($"tv2: {tv.Item2}");

            //foreach (var t in tv.Item2)
            //{
            //    _output.WriteLine($"tv: {t}");
            //}

            var times = Enumerable.Range(0, 8).Select(s =>
            {
                return DateTime.Now.AddMinutes(s * 3);
                
            }).ToList();

            foreach (var t in times)
            {
                _output.WriteLine($"t: {t}");
            }


           
            var start = times.Min();
            var end = times.Max();
            var size = 15;

            //var huh = Convert.ToInt32((end.TimeOfDay.TotalMinutes - start.TimeOfDay.TotalMinutes) / size + 1);

            //_output.WriteLine($"huh: {huh}");

            //var values = Enumerable
            //    .Range(0, Convert.ToInt32((end.TimeOfDay.TotalMinutes - start.TimeOfDay.TotalMinutes) / size + 1))
            //    .Select((s, i) => start.AddMinutes(i * size)).ToList();

            //foreach (var v in values)
            //{
            //    _output.WriteLine($"v: {v}");
            //}

            //var boo = values.Take(values.Count() - 1).Select((s, i) => new StartEndRange() { Start = values[i], End = values[i + 1] });

            
            var chunk = TimeSpan.FromMinutes(size);

            start = times.Min().RoundDown(chunk);
            end = times.Max().RoundUp(chunk);

            var test = end - start;

            _output.WriteLine($"start: {start}");
            _output.WriteLine($"test: {end}");

            _output.WriteLine($"test: {test}");
            _output.WriteLine($"test: {Convert.ToInt32(test.Divide(chunk))}");


            var values = Enumerable
                .Range(0, Convert.ToInt32(test.Divide(chunk)) + 1)
                .Select((s, i) => start.Add(chunk.Multiply(i))).ToList();


            foreach (var v in values)
            {
                _output.WriteLine($"v: {v}");
            }

            var boo = values.Take(values.Count() - 1).Select((s, i) => new StartEndRange() { Start = values[i], End = values[i + 1] });

            foreach (var b in boo)
            {
                _output.WriteLine($"b: {b.Start} - {b.End}");
            }
























            //foreach (var a in signal.Approaches)
            //{
            //    var o = signal.Approaches.FirstOrDefault(w => w.DirectionTypeId == new OpposingDirection(a.DirectionTypeId));

            //    _output.WriteLine($"primary: {a} opposing: {o}");

            //    if (o != null )
            //    {
            //        var boo = new TempPhaseDirections(logs) { Primary = a, Opposing = o };

            //        _output.WriteLine($"primary: {boo.Primary} logs: {boo.PrimaryLogs.Count}");
            //        _output.WriteLine($"opposing: {boo.Opposing} logs: {boo.OpposingLogs.Count}");
            //    }





            //   //foreach (var s in stuff)
            //   // {
            //   //     if (s.Item2.Count() > 0)
            //   //         _output.WriteLine($"stuff: {s.Item1.DetectorChannel} logs: {s.Item2.Count()} - {s.Item2.All(a => a.EventParam == s.Item1.DetectorChannel)}");
            //   // }


            //}






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
