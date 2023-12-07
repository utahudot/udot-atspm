using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Workflows;
using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
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
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Moq;
using Xunit.Abstractions;

namespace TempReportTests.IReportServiceTests
{
    //https://github.com/pengweiqhca/Xunit.DependencyInjection
    public class Startup
    {
        public void ConfigureServices(IServiceCollection s)
        {
            //s.AddLogging(s => s..AddXunitOutput());

            //IEnumerable<ControllerEventLog>, ApproachDelayResult

            //s.AddTransient<IExecuteWithProgress<IEnumerable<ControllerEventLog>, IAsyncEnumerable<ATSPM.Application.Analysis.ApproachDelay.ApproachDelayResult>, int>, ApproachDelayWorkflow>();

            s.AddScoped(s => Mock.Of<ISignalRepository>());
            s.AddScoped(s => Mock.Of<IControllerEventLogRepository>());

            s.AddScoped<IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>, ApproachDelayReportService>();
            //s.AddScoped<IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>, ApproachDelayWorkflowReport>();

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

        //private readonly IExecuteWithProgress<IEnumerable<ControllerEventLog>, IAsyncEnumerable<ATSPM.Application.Analysis.ApproachDelay.ApproachDelayResult>, int> _workflow;

        private readonly IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>> _service;

        //private readonly ApproachDelayService _approachDelayService;
        //private readonly SignalPhaseService _signalPhaseService;
        private readonly ISignalRepository _signalRepository;
        private readonly IControllerEventLogRepository _controllerEventLogRepository;
        //private readonly PhaseService _phaseService;
        private readonly TestDataUtility _testDataUtility;

        //private List<ControllerEventLog> logs;

        public ApproachDelayReportServiceTests(
            ITestOutputHelper output,

            //IExecuteWithProgress<IEnumerable<ControllerEventLog>, IAsyncEnumerable<ATSPM.Application.Analysis.ApproachDelay.ApproachDelayResult>, int> workflow,

            IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>> service,
            //ApproachDelayService approachDelayService,
            //SignalPhaseService signalPhaseService,
            ISignalRepository signalRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            //PhaseService phaseService,
            TestDataUtility testDataUtility
            )
        {
            _output = output;

            //_workflow = workflow;

            _service = service;
            //_approachDelayService = approachDelayService;
            //_signalPhaseService = signalPhaseService;
            _signalRepository = signalRepository;
            _controllerEventLogRepository = controllerEventLogRepository;
            //_phaseService = phaseService;
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
        public async void ApproachDelayWorkflowTest()
        {
            var data = _testDataUtility.ReadTestFile<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\TempReportTests\TestFiles\7115-ApproachDelayOptions-ReportTestData.json"));

            Mock.Get(_signalRepository).Setup(s => s.GetLatestVersionOfSignal(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns<string, DateTime>((a, b) => data.Signal)
                .Callback<string, DateTime>((a, b) => _output.WriteLine($"stuff just happened {a} - {b}"));

            Mock.Get(_controllerEventLogRepository).Setup(s => s.GetSignalEventsBetweenDates(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(() => data.Logs)
                .Callback<string, DateTime, DateTime>((a, b, c) => _output.WriteLine($"returned logs for {a} - {b} - {c}"));

            var sut = _service;

            var result = await sut.ExecuteAsync(data.Options, null, default);

            foreach (var r in result)
            {
                _output.WriteLine($"------------------------------------result! {r}");
            }
        }

        [Fact]
        public async void ExecuteAsyncPass()
        {
            var data = _testDataUtility.ReadTestFile<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\TempReportTests\TestFiles\7115-ApproachDelayOptions-ReportTestData.json"));

            var options = data.Options;

            Mock.Get(_signalRepository).Setup(s => s.GetLatestVersionOfSignal(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns<string, DateTime>((a, b) => data.Signal)
                .Callback<string, DateTime>((a, b) => _output.WriteLine($"stuff just happened {a} - {b}"));

            Mock.Get(_controllerEventLogRepository).Setup(s => s.GetSignalEventsBetweenDates(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(() => data.Logs)
                .Callback<string, DateTime, DateTime>((a, b, c) => _output.WriteLine($"returned logs for {a} - {b} - {c}"));


            var sut = _service;

            var stuff = await _service.ExecuteAsync(options, null, default);



            _output.WriteLine($"------------------------------------output! {stuff.Count()}");

            Assert.True(true);
        }

        public void Dispose()
        {
        }
    }

    public class ApproachDelayWorkflowReport : ReportServiceBase<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>
    {
        private readonly ISignalRepository _signalRepository;
        private readonly IControllerEventLogRepository _controllerEventLogRepository;

        public ApproachDelayWorkflowReport(ISignalRepository signalRepository, IControllerEventLogRepository controllerEventLogRepository)
        {
            _signalRepository = signalRepository;
            _controllerEventLogRepository = controllerEventLogRepository;
        }

        public override async Task<IEnumerable<ApproachDelayResult>> ExecuteAsync(ApproachDelayOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var result = new List<ApproachDelayResult>();
            
            var signal = _signalRepository.GetLatestVersionOfSignal(parameter.SignalIdentifier, parameter.Start);

            if (signal == null)
            {
                //return BadRequest("Signal not found");
                return await Task.FromException<IEnumerable<ApproachDelayResult>>(new NullReferenceException("Signal not found"));
            }

            var controllerEventLogs = _controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<IEnumerable<ApproachDelayResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var workflow = new ApproachDelayWorkflow();

            //await foreach (var r in workflow.Execute(controllerEventLogs, null, default))
            //{
            //    var test = new ApproachDelayResult()
            //    {
            //        PhaseNumber = r.PhaseNumber,
            //        PhaseDescription = "Figure this out",
            //        ApproachDescription = "Figure this out",
            //        AverageDelayPerVehicle = r.AverageDelay,
            //        TotalDelay = r.TotalDelay,
            //        ApproachId = 0,
            //        Start = parameter.Start,
            //        End = parameter.End,
            //        SignalIdentifier = r.SignalIdentifier,
            //        SignalDescription = signal.ToString(),
            //        //Plans = r.Plans
            //    };

            //    test.ApproachDelayDataPoints = new List<DataPointForDouble>();

            //    var tl = Timeline.FromMinutes<StartEndRange>(parameter.Start, parameter.End, parameter.BinSize);

            //    foreach (var t in tl)
            //    {
            //        Console.WriteLine($"timeline: {t.Start} - {t.End}");

            //        var dp = new DataPointForDouble(t.Start, r.Vehicles.Where(w => t.InRange(w.CorrectedTimeStamp)).Sum(s => s.Delay) / 3600);

            //        test.ApproachDelayDataPoints.Add(dp);
            //    }


            //    //public double TotalDelay => Vehicles.Sum(s => s.Delay) / 3600;



            //    result.Add(test);
            //}

            return result;
        }
    }
}