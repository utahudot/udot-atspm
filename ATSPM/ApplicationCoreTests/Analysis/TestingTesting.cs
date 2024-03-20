using ApplicationCoreTests.Analysis.TestObjects;
using ATSPM.Application;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.Workflows;
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
using System.Diagnostics;
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

namespace ApplicationCoreTests.Analysis
{
    public class PreemptCycle1 : PreempDetailValueBase
    {
        public DwellTimeValue DwellTime { get; set; }
        public TrackClearTimeValue TrackClearTime { get; set; }
        public TimeToServiceValue ServiceTime { get; set; }
        public DelayTimeValue Delay { get; set; }
        public TimeToGateDownValue GateDownTime { get; set; }
        public TimeToCallMaxOutValue CallMaxOutTime { get; set; }

        public bool HasDelay => Delay?.Seconds.Seconds > 0;
    }



    public class TestingTesting
    {
        private readonly ITestOutputHelper _output;

        public TestingTesting(ITestOutputHelper output)
        {
            _output = output;
        }

        //IEnumerable<T> PreemptDetailRange<T>(IEnumerable<ControllerEventLog> items, IndianaEnumerations first, IndianaEnumerations second) where T : PreempDetailValueBase, new()
        //{
        //    var result = items.GroupBy(g => g.locationIdentifier, (Location, l1) =>
        //    l1.GroupBy(g => g.EventParam, (preempt, l2) =>
        //    l2.TimeSpanFromConsecutiveCodes(first, second)
        //    .Select(s => new T()
        //    {
        //        locationIdentifier = Location,
        //        PreemptNumber = preempt,
        //        Start = s.Item1[0].Timestamp,
        //        End = s.Item1[1].Timestamp,
        //        Seconds = s.Item2
        //    })).SelectMany(m => m)).SelectMany(m => m);

        //    return result;
        //}

        [Fact]
        public async void TestingStuff()
        {
            //var filePath = @"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData";

            //var logs1 = ControllerEventLogHelper.ImportLogsFromCsvFile(Path.Combine(filePath, "7115TerminationData.csv"));
            ////var logs2 = ControllerEventLogHelper.ImportLogsFromCsvFile(Path.Combine(filePath, "7706PreemptData.csv"));

            //var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Location7115TestData.json").FullName);
            //var Location = JsonConvert.DeserializeObject<Location>(json);
































            //var logs = new List<ControllerEventLog>
            //{
            //    new ControllerEventLog() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
            //    new ControllerEventLog() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
            //    new ControllerEventLog() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
            //    new ControllerEventLog() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:23:01.2"), EventCode = 111, EventParam = 1},

            //    new ControllerEventLog() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 13:01:01.1"), EventCode = 102, EventParam = 1},
            //    new ControllerEventLog() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 13:02:01.1"), EventCode = 105, EventParam = 1},
            //    new ControllerEventLog() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 13:03:01.1"), EventCode = 104, EventParam = 1},
            //    new ControllerEventLog() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 13:23:01.2"), EventCode = 111, EventParam = 1},
            //};























            //var test2 = logs.Select((w, i) => logs.FindIndex(i, 1, f => f.EventCode == 111)).Where(w => w > 0);

            //foreach (var e in test2)
            //{
            //    _output.WriteLine($"test2: {e}");
            //}


            //foreach (var e in test3)
            //{
            //    _output.WriteLine($"test3: {e}");
            //}

            //var test4 = test2.Select((s, i) => new Range(i == 0 ? 0 : test2[i - 1] + 1, i < test2.Count() - 1 ? test2[i + 1] - test2[i] : test2[i] - test2[i - 1])).ToList();

            //foreach (var e in test4)
            //{
            //    _output.WriteLine($"test4: {e}");
            //}

            //var test4 = test2.Select((s, i) => logs.GetRange(i == 0 ? 0 : test2[i - 1] + 1, i < test2.Count() - 1 ? test2[i + 1] - test2[i] : test2[i] - test2[i - 1])).ToList();

            //foreach (var e in test4)
            //{
            //    _output.WriteLine($"test4: {e}");
            //}

            //var result = test4.Select(s => new PreemptCycle()
            //{
            //    InputOff = s.FindAll(f => f.EventCode == 104).Select(s => s.Timestamp).ToList(),
            //    //InputOn = s.FindAll(f => f.EventCode == 102).Select(s => s.Timestamp).ToList(),
            //    StartInputOn = s.Find(f => f.EventCode == 102)?.Timestamp ?? default,
            //    GateDown = s.Find(f => f.EventCode == 103)?.Timestamp ?? default,
            //    EntryStarted = s.Find(f => f.EventCode == 104)?.Timestamp ?? default,
            //    BeginTrackClearance = s.Find(f => f.EventCode == 106)?.Timestamp ?? default,
            //    BeginDwellService = s.Find(f => f.EventCode == 107)?.Timestamp ?? default,
            //    MaxPresenceExceeded = s.Find(f => f.EventCode == 110)?.Timestamp ?? default,
            //    BeginExitInterval = s.Find(f => f.EventCode == 110)?.Timestamp ?? default,
            //});



            //foreach (var e in result)
            //{
            //    _output.WriteLine($"e: {e}");
            //}




            //var cycles = PreemptDetailRange<PreemptCycle1>(logs, IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptionBeginExitInterval);

            //var dwell = PreemptDetailRange<DwellTimeValue>(logs, IndianaEnumerations.PreemptionBeginDwellService, IndianaEnumerations.PreemptionBeginExitInterval);
            //var trackclear = PreemptDetailRange<TrackClearTimeValue>(logs, IndianaEnumerations.PreemptionBeginTrackClearance, IndianaEnumerations.PreemptionBeginDwellService);
            //var timetoservice = PreemptDetailRange<TimeToServiceValue>(logs, IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptionBeginDwellService);
            //var delay = PreemptDetailRange<DelayTimeValue>(logs, IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptEntryStarted);
            //var gatedown = PreemptDetailRange<TimeToGateDownValue>(logs, IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptGateDownInputReceived);
            //var maxout = PreemptDetailRange<TimeToCallMaxOutValue>(logs, IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptionMaxPresenceExceeded);


            //foreach (var c in cycles)
            //{
            //    c.DwellTime = dwell.FirstOrDefault(w => c.InRange(w));
            //    c.TrackClearTime = trackclear.FirstOrDefault(w => c.InRange(w));
            //    c.ServiceTime = timetoservice.FirstOrDefault(w => c.InRange(w));
            //    c.Delay = delay.FirstOrDefault(w => c.InRange(w));
            //    c.GateDownTime = gatedown.FirstOrDefault(w => c.InRange(w));
            //    c.CallMaxOutTime = maxout.FirstOrDefault(w => c.InRange(w));

            //    _output.WriteLine($"c: {c}");
            //}



            //var codes = new List<int>() { 102, 103, 104, 105, 107, 110, 111 };

            //var cycle = logs.TimeSpanFromConsecutiveCodes(IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptionBeginExitInterval);
            //var dwell = logs.TimeSpanFromConsecutiveCodes(IndianaEnumerations.PreemptionBeginDwellService, IndianaEnumerations.PreemptionBeginExitInterval);
            //var trackclear = logs.TimeSpanFromConsecutiveCodes(IndianaEnumerations.PreemptionBeginTrackClearance, IndianaEnumerations.PreemptionBeginDwellService);
            //var timetoservice = logs.TimeSpanFromConsecutiveCodes(IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptionBeginDwellService);
            //var delay = logs.TimeSpanFromConsecutiveCodes(IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptEntryStarted);
            //var gatdown = logs.TimeSpanFromConsecutiveCodes(IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptGateDownInputReceived);
            //var maxout = logs.TimeSpanFromConsecutiveCodes(IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptionMaxPresenceExceeded);

            //var cycles = cycle.Select(s => new PreemptDetailResult()
            //{
            //    Start = s.Item1[0].Timestamp,
            //    End = s.Item1[1].Timestamp,
            //    Seconds = s.Item2
            //}).ToList();

            //cycles.ForEach(f =>
            //{
            //    //f.locationIdentifier = Location.Key;
            //    //f.PreemptNumber = item.Key;
            //    //f.Start = item.Min(m => m.Start);
            //    //f.End = item.Max(m => m.End);
            //    f.DwellTimes = item.Where(w => w.GetType().Name == nameof(DwellTimeValue)).Cast<DwellTimeValue>().ToList();
            //            f.TrackClearTimes = item.Where(w => w.GetType().Name == nameof(TrackClearTimeValue)).Cast<TrackClearTimeValue>().ToList();
            //            f.ServiceTimes = item.Where(w => w.GetType().Name == nameof(TimeToServiceValue)).Cast<TimeToServiceValue>().ToList();
            //            f.Delay = item.Where(w => w.GetType().Name == nameof(DelayTimeValue)).Cast<DelayTimeValue>().ToList();
            //            f.GateDownTimes = item.Where(w => w.GetType().Name == nameof(TimeToGateDownValue)).Cast<TimeToGateDownValue>().ToList();
            //    f.CallMaxOutTimes = item.Where(w => w.GetType().Name == nameof(TimeToCallMaxOutValue)).Cast<TimeToCallMaxOutValue>().ToList();
            //});









            //var filteredPreemptionData = new FilteredPreemptionData();
            //var groupEventLogsByParameter = new GroupEventLogsByParameter();
            //var filterLogsByLocationAndParamter = new BroadcastBlock<Tuple<Location, IEnumerable<ControllerEventLog>, int>>(f =>
            //{
            //    return Tuple.Create(f.Item1, f.Item2.FromSpecification(new ControllerLogLocationAndParamterFilterSpecification(f.Item1, f.Item3)), f.Item3);
            //});

            //var options = new ExecutionDataflowBlockOptions()
            //{
            //    BoundedCapacity = 6
            //};


            //var calculateDwellTime = new CalculateDwellTime(options);
            //var calculateTrackClearTime = new CalculateTrackClearTime(options);
            //var calculateTimeToService = new CalculateTimeToService(options);
            //var calculateDelay = new CalculateDelay(options);
            //var calculateTimeToGateDown = new CalculateTimeToGateDown(options);
            //var calculateTimeToCallMaxOut = new CalculateTimeToCallMaxOut(options);

            //var batchPrempt = new BatchBlock<PreempDetailValueBase>(6);

            ////var generatePreemptDetailResults = new GeneratePreemptDetailResults();
            //var generatePreemptDetailResults = new TransformManyBlock<PreempDetailValueBase[], PreemptDetailResult>(f =>
            //{
            //    _output.WriteLine($"f: {f.Length}");

            //    foreach (var v in f)
            //    {
            //        _output.WriteLine($"v: {v.GetType().Name} --- {v}");
            //    }

            //    return f.Select(s => new PreemptDetailResult()
            //    {
            //        //locationIdentifier = Location.Key,
            //        //PreemptNumber = item.Key,
            //        //Start = item.Min(m => m.Start),
            //        //End = item.Max(m => m.End),
            //        //DwellTimes = item.Where(w => w.GetType().Name == nameof(DwellTimeValue)).Cast<DwellTimeValue>().ToList(),
            //        //TrackClearTimes = item.Where(w => w.GetType().Name == nameof(TrackClearTimeValue)).Cast<TrackClearTimeValue>().ToList(),
            //        //ServiceTimes = item.Where(w => w.GetType().Name == nameof(TimeToServiceValue)).Cast<TimeToServiceValue>().ToList(),
            //        //Delay = item.Where(w => w.GetType().Name == nameof(DelayTimeValue)).Cast<DelayTimeValue>().ToList(),
            //        //GateDownTimes = item.Where(w => w.GetType().Name == nameof(TimeToGateDownValue)).Cast<TimeToGateDownValue>().ToList(),
            //        //CallMaxOutTimes = item.Where(w => w.GetType().Name == nameof(TimeToCallMaxOutValue)).Cast<TimeToCallMaxOutValue>().ToList()
            //    });
            //});


            //var resultAction = new ActionBlock<PreemptDetailResult>(a =>
            //{
            //    //_output.WriteLine($"a: {a}");
            //});




            //filteredPreemptionData.LinkTo(groupEventLogsByParameter, new DataflowLinkOptions() { PropagateCompletion = true });
            //groupEventLogsByParameter.LinkTo(filterLogsByLocationAndParamter, new DataflowLinkOptions() { PropagateCompletion = true });


            //filterLogsByLocationAndParamter.LinkTo(calculateDwellTime, new DataflowLinkOptions() { PropagateCompletion = true, MaxMessages = 3});
            //filterLogsByLocationAndParamter.LinkTo(calculateTrackClearTime, new DataflowLinkOptions() { PropagateCompletion = true });
            //filterLogsByLocationAndParamter.LinkTo(calculateTimeToService, new DataflowLinkOptions() { PropagateCompletion = true });
            //filterLogsByLocationAndParamter.LinkTo(calculateDelay, new DataflowLinkOptions() { PropagateCompletion = true });
            //filterLogsByLocationAndParamter.LinkTo(calculateTimeToGateDown, new DataflowLinkOptions() { PropagateCompletion = true });
            //filterLogsByLocationAndParamter.LinkTo(calculateTimeToCallMaxOut, new DataflowLinkOptions() { PropagateCompletion = true });


            //calculateDwellTime.LinkTo(batchPrempt, new DataflowLinkOptions() { PropagateCompletion = true });
            //calculateTrackClearTime.LinkTo(batchPrempt, new DataflowLinkOptions() { PropagateCompletion = true });
            //calculateTimeToService.LinkTo(batchPrempt, new DataflowLinkOptions() { PropagateCompletion = true });
            //calculateDelay.LinkTo(batchPrempt, new DataflowLinkOptions() { PropagateCompletion = true });
            //calculateTimeToGateDown.LinkTo(batchPrempt, new DataflowLinkOptions() { PropagateCompletion = true });
            //calculateTimeToCallMaxOut.LinkTo(batchPrempt, new DataflowLinkOptions() { PropagateCompletion = true });

            //batchPrempt.LinkTo(generatePreemptDetailResults, new DataflowLinkOptions() { PropagateCompletion = true });

            //generatePreemptDetailResults.LinkTo(resultAction, new DataflowLinkOptions() { PropagateCompletion = true });


            //filteredPreemptionData.Post(Tuple.Create(Location1, logs3));

            //filteredPreemptionData.Complete();

            //await resultAction.Completion;

























            //var json = JsonConvert.SerializeObject(logs);

            //File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\ApproachVolumeTestLogs.json", json);

            //var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Location7115TestData.json").FullName);
            //var Location = JsonConvert.DeserializeObject<Location>(json);

            //var json1 = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\RawCycleData.json").FullName);
            //var json2 = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculatePhaseVolumeTestData2.json").FullName);

            //var data1 = JsonConvert.DeserializeObject<List<ControllerEventLog>>(json1);
            //var data2 = JsonConvert.DeserializeObject<CalculatePhaseVolumeTestData>(json2);

            ////Location.Approaches.Clear();
            ////Location.Approaches.Add(data1.Configuration);
            ////Location.Approaches.Add(data2.Configuration);





            //var t1 = Tuple.Create(Location.Approaches.FirstOrDefault(f => f.Id == 2880), data1.AsEnumerable());
            ////var t2 = Tuple.Create(data2.Configuration, data2.Output);



        }

        //private async void TempGeneratePreemtTestData()
        //{
        //    var file1 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\PreemptDetaildata.csv");

        //    var logs = File.ReadAllLines(file1.FullName)
        //           .Skip(1)
        //           .Select(x => x.Split(','))
        //           .Select(x => new ControllerEventLog
        //           {
        //               locationIdentifier = x[0],
        //               Timestamp = DateTime.Parse(x[1]),
        //               EventCode = int.Parse(x[2]),
        //               EventParam = int.Parse(x[3])
        //           }).ToList();

        //    var Location = new Location()
        //    {
        //        locationIdentifier = "7573",
        //        PrimaryName = "Test Controller"
        //    };

        //    var c = new CalculateDwellTime();

        //    var r = await c.ExecuteAsync(Tuple.Create(Location, logs.AsEnumerable(), 1));

        //    var result = new PreemptiveProcessTestData()
        //    {
        //        Configuration = Location,
        //        Input = logs,
        //        Output = r.Item2.Cast<PreempDetailValueBase>().ToList()
        //    };


        //    var test = JsonConvert.SerializeObject(result, new JsonSerializerSettings()
        //    {
        //        TypeNameHandling = TypeNameHandling.All
        //    });
        //    File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculateDwellTimeTestData1.json", test);
        //}
    }

    

}
