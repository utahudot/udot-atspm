﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - ApplicationCoreTests.Analysis/TestingTesting.cs
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

using Utah.Udot.Atspm.Analysis.PreemptionDetails;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis
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

        //IEnumerable<T> PreemptDetailRange<T>(IEnumerable<IndianaEvent> items, IndianaEnumerations first, IndianaEnumerations second) where T : PreempDetailValueBase, new()
        //{
        //    var result = items.GroupBy(g => g.LocationIdentifier, (Location, l1) =>
        //    l1.GroupBy(g => g.EventParam, (preempt, l2) =>
        //    l2.TimeSpanFromConsecutiveCodes(first, second)
        //    .Select(s => new T()
        //    {
        //        LocationIdentifier = Location,
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

            //var logs1 = IndianaEventHelper.ImportLogsFromCsvFile(Path.Combine(filePath, "7115TerminationData.csv"));
            ////var logs2 = IndianaEventHelper.ImportLogsFromCsvFile(Path.Combine(filePath, "7706PreemptData.csv"));

            //var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\Location7115TestData.json").FullName);
            //var Location = JsonConvert.DeserializeObject<Location>(json);
































            //var logs = new List<IndianaEvent>
            //{
            //    new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
            //    new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
            //    new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
            //    new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:23:01.2"), EventCode = 111, EventParam = 1},

            //    new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 13:01:01.1"), EventCode = 102, EventParam = 1},
            //    new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 13:02:01.1"), EventCode = 105, EventParam = 1},
            //    new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 13:03:01.1"), EventCode = 104, EventParam = 1},
            //    new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 13:23:01.2"), EventCode = 111, EventParam = 1},
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
            //var maxout = PreemptDetailRange<TimeToCallMaxOutValue>(logs, IndianaEnumerations.PreemptCallInputOn, 110);


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
            //var maxout = logs.TimeSpanFromConsecutiveCodes(IndianaEnumerations.PreemptCallInputOn, 110);

            //var cycles = cycle.Select(s => new PreemptDetailResult()
            //{
            //    Start = s.Item1[0].Timestamp,
            //    End = s.Item1[1].Timestamp,
            //    Seconds = s.Item2
            //}).ToList();

            //cycles.ForEach(f =>
            //{
            //    //f.LocationIdentifier = Location.Key;
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
            //var filterLogsByLocationAndParamter = new BroadcastBlock<Tuple<Location, IEnumerable<IndianaEvent>, int>>(f =>
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
            //        //LocationIdentifier = Location.Key,
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

            //var data1 = JsonConvert.DeserializeObject<List<IndianaEvent>>(json1);
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
        //           .Select(x => new IndianaEvent
        //           {
        //               LocationIdentifier = x[0],
        //               Timestamp = DateTime.Parse(x[1]),
        //               EventCode = int.Parse(x[2]),
        //               EventParam = int.Parse(x[3])
        //           }).ToList();

        //    var Location = new Location()
        //    {
        //        LocationIdentifier = "7573",
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
