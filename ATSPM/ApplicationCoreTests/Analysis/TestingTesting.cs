using ApplicationCoreTests.Analysis.TestObjects;
using ATSPM.Application;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Common;
using ATSPM.Application.Extensions;
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


            var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Signal7115.json").FullName);
            var signal = JsonConvert.DeserializeObject<Signal>(json);

            var json1 = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\ApproachVolumeTestLogs.json").FullName);
            var logs = JsonConvert.DeserializeObject<IEnumerable<ControllerEventLog>>(json1);


            _output.WriteLine($"signal: {signal}");

            var primaryChain = new IdentifyandAdjustVehicleActivations();
            var opposingChain = new IdentifyandAdjustVehicleActivations();
            var VolumeP = new CalculatePhaseVolume();
            var VolumeO = new CalculatePhaseVolume();

            var joinVolumes = new JoinBlock<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>>();

            var totalVolume = new CalculateTotalVolumes();

            var testResults = new ActionBlock<Tuple<Approach, TotalVolumes>>(a =>
            {
                _output.WriteLine($"a: {a.Item1}");

                foreach (var v in a.Item2)
                {
                    _output.WriteLine($"v: {v}");
                }
            });

            primaryChain.LinkTo(VolumeP, new DataflowLinkOptions() { PropagateCompletion = true });
            opposingChain.LinkTo(VolumeO, new DataflowLinkOptions() { PropagateCompletion = true });

            VolumeP.LinkTo(joinVolumes.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            VolumeO.LinkTo(joinVolumes.Target2, new DataflowLinkOptions() { PropagateCompletion = true });

            joinVolumes.LinkTo(totalVolume, new DataflowLinkOptions() { PropagateCompletion = true });

            totalVolume.LinkTo(testResults, new DataflowLinkOptions() { PropagateCompletion = true });


            var stuff = signal.Approaches.SelectMany(m => m.Detectors).GroupJoin(logs, d => d.DetectorChannel, l => l.EventParam, (o, i) => Tuple.Create(signal.Approaches.First(f => f.Id == o.Approach.Id), i)).Where(w => w.Item2.Any());

            foreach (var app in stuff)
            {
                var o = stuff.FirstOrDefault(w => w.Item1.DirectionTypeId == new OpposingDirection(app.Item1.DirectionTypeId));
                _output.WriteLine($"primary: {app.Item1}:{app.Item2.Count()} opposing: {o}");


                primaryChain.Post(app);
                opposingChain.Post(o);

                primaryChain.Complete();
                opposingChain.Complete();
            }

            await testResults.Completion;



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
