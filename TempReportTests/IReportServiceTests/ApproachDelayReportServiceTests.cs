using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Infrastructure.Repositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.AppoachDelay;
using ATSPM.ReportApi.Business.ApproachSpeed;
using ATSPM.ReportApi.Business.ApproachVolume;
using ATSPM.ReportApi.Business.ArrivalOnRed;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.GreenTimeUtilization;
using ATSPM.ReportApi.Business.LeftTurnGapAnalysis;
using ATSPM.ReportApi.Business.LeftTurnGapReport;
using ATSPM.ReportApi.Business.PedDelay;
using ATSPM.ReportApi.Business.PreempDetail;
using ATSPM.ReportApi.Business.PreemptService;
using ATSPM.ReportApi.Business.PreemptServiceRequest;
using ATSPM.ReportApi.Business.PurdueCoordinationDiagram;
using ATSPM.ReportApi.Business.SplitFail;
using ATSPM.ReportApi.Business.SplitMonitor;
using ATSPM.ReportApi.Business.TimingAndActuation;
using ATSPM.ReportApi.Business.TurningMovementCounts;
using ATSPM.ReportApi.Business.WaitTime;
using ATSPM.ReportApi.Business.YellowRedActivations;
using ATSPM.ReportApi.ReportServices;
using Google.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Moq;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace TempReportTests.IReportServiceTests
{
    //https://github.com/pengweiqhca/Xunit.DependencyInjection
    public class Startup
    {
        public void ConfigureServices(IServiceCollection s)
        {
            //s.AddLogging(s => s..AddXunitOutput());

            s.AddScoped(s => Mock.Of<ISignalRepository>());
            s.AddScoped(s => Mock.Of<IControllerEventLogRepository>());

            s.AddScoped<IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>, ApproachDelayReportService>();

            s.AddScoped<TestDataUtility>();

            //Chart Services
            s.AddScoped<ApproachDelayService>();
            s.AddScoped<ApproachSpeedService>();
            s.AddScoped<ApproachVolumeService>();
            s.AddScoped<ArrivalOnRedService>();
            s.AddScoped<LeftTurnGapAnalysisService>();
            s.AddScoped<LeftTurnReportPreCheckService>();
            s.AddScoped<LeftTurnVolumeAnalysisService>();
            s.AddScoped<PedDelayService>();
            s.AddScoped<GreenTimeUtilizationService>();
            s.AddScoped<PreemptServiceService>();
            s.AddScoped<PreemptServiceRequestService>();
            s.AddScoped<PurdueCoordinationDiagramService>();
            s.AddScoped<SplitFailPhaseService>();
            s.AddScoped<SplitMonitorService>();
            s.AddScoped<TimingAndActuationsForPhaseService>();
            s.AddScoped<TurningMovementCountsService>();
            s.AddScoped<WaitTimeService>();
            s.AddScoped<YellowRedActivationsService>();

            //Common Services
            s.AddScoped<PlanService>();
            s.AddScoped<SignalPhaseService>();
            s.AddScoped<CycleService>();
            s.AddScoped<PedPhaseService>();
            s.AddScoped<AnalysisPhaseCollectionService>();
            s.AddScoped<AnalysisPhaseService>();
            s.AddScoped<PreemptDetailService>();
            s.AddScoped<PhaseService>();
        }
    }

    public class ApproachDelayReportServiceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private readonly IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>> _service;

        private readonly ApproachDelayService _approachDelayService;
        private readonly SignalPhaseService _signalPhaseService;
        private readonly ISignalRepository _signalRepository;
        private readonly IControllerEventLogRepository _controllerEventLogRepository;
        private readonly PhaseService _phaseService;
        private readonly TestDataUtility _testDataUtility;

        //private List<ControllerEventLog> logs;

        public ApproachDelayReportServiceTests(
            ITestOutputHelper output,
            IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>> service,
            ApproachDelayService approachDelayService,
            SignalPhaseService signalPhaseService,
            ISignalRepository signalRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            PhaseService phaseService,
            TestDataUtility testDataUtility
            )
        {
            _output = output;

            _service = service;
            _approachDelayService = approachDelayService;
            _signalPhaseService = signalPhaseService;
            _signalRepository = signalRepository;
            _controllerEventLogRepository = controllerEventLogRepository;
            _phaseService = phaseService;
            _testDataUtility = testDataUtility;


            //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "ControllerEvents-ApproachDelay.csv");

            //_output.WriteLine($"path: {path}");

            //var logs = File.ReadAllLines(path)
            //               .Skip(1)
            //               .Select(x => x.Split(','))
            //               .Select(x => new ControllerEventLog
            //               {
            //                   SignalIdentifier = x[0],
            //                   Timestamp = DateTime.Parse(x[1]),
            //                   EventCode = int.Parse(x[2]),
            //                   EventParam = int.Parse(x[3])
            //               }).ToList();

            //_output.WriteLine($"log count: {logs.Count}");
        }

        [Fact]
        public async void ExecuteAsyncPass()
        {
            var data = _testDataUtility.ReadTestFile<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\TempReportTests\TestFiles\7115-ApproachDelayOptions-ReportTestData.json"));



            //var test = File.ReadAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\TempReportTests\TestFiles\7115-ApproachDelayReportService.json");


            //var data = JsonSerializer.Deserialize<ReportServiceData<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>>(test);

            var options = data.Options;


            //var options = new ApproachDelayOptions() 
            //{ 
            //    SignalIdentifier = "7115", 
            //    BinSize = 15, 
            //    Start = new DateTime(2023, 4, 17, 8, 0, 0), 
            //    End = new DateTime(2023, 4, 17, 9, 0, 0), 
            //    GetVolume = true 
            //};

            Mock.Get(_signalRepository).Setup(s => s.GetLatestVersionOfSignal(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns<string, DateTime>((a, b) => data.Signal)
                .Callback<string, DateTime>((a, b) => _output.WriteLine($"stuff just happened {a} - {b}"));

            Mock.Get(_controllerEventLogRepository).Setup(s => s.GetSignalEventsBetweenDates(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(() => data.Logs)
                .Callback<string, DateTime, DateTime>((a, b, c) => _output.WriteLine($"returned logs for {a} - {b} - {c}"));



            //var test1 = _signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            //var test2 = _controllerEventLogRepository.GetSignalEventsBetweenDates(test1.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12));



            //_output.WriteLine($"log count: {test2.Count}");

            var sut = _service;

            var stuff = await _service.ExecuteAsync(options, null, default);



            _output.WriteLine($"------------------------------------output! {stuff.Count()}");

            Assert.True(true);
        }

        public void Dispose()
        {
        }
    }
}