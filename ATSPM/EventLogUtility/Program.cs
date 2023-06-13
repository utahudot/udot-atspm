using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Configuration;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks.Dataflow;
using ATSPM.Application.Analysis.ApproachVolume;
using ATSPM.Domain.Common;
using System.CommandLine;
using ATSPM.Application;
using Microsoft.AspNetCore.Mvc;
using ATSPM.Application.Analysis.PurdueCoordination;
using System.Text.Json;
using ATSPM.Application.Analysis.PreemptionDetails;
using System.Collections.Generic;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.WorkflowFilters;


//var path1 = "C:\\temp\\TestData\\7115_Approach_Delay.csv";
//var path2 = "C:\\temp\\TestData\\1001_Approach_Delay.csv";

var path1 = "C:\\temp\\TestData\\7115_Approach_Delay.csv";

var list = File.ReadAllLines(path1)
               .Skip(1)
               .Select(x => x.Split(','))
               .Select(x => new ControllerEventLog
               {
                   SignalId = x[0],
                   Timestamp = DateTime.Parse(x[1]),
                   EventCode = int.Parse(x[2]),
                   EventParam = int.Parse(x[3])
               }).ToList();


var path2 = "C:\\temp\\TestData\\7115_4-17-2023_Plans.csv";

var list2 = File.ReadAllLines(path2)
               .Skip(1)
               .Select(x => x.Split(','))
               .Select(x => new ControllerEventLog
               {
                   SignalId = x[0],
                   Timestamp = DateTime.Parse(x[1]),
                   EventCode = int.Parse(x[2]),
                   EventParam = int.Parse(x[3])
               }).ToList();

Console.WriteLine($"list2: {list2.Count}");

list = list.Union(list2).ToList();

Console.WriteLine($"list2: {list2.Where(w => w.EventCode == 131).Count()}");



//var s = new Signal() { SignalId = "7191" };


//var d1 = new Detector()
//{
//    DetChannel = 2,
//    DistanceFromStopBar = 340,
//    LatencyCorrection = 0,
//    Approach = new Approach()
//    {
//        ProtectedPhaseNumber = 2,
//        DirectionTypeId = DirectionTypes.NB,
//        Mph = 45,
//        Signal = s
//    }
//};

//var d2 = new Detector()
//{
//    DetChannel = 4,
//    DistanceFromStopBar = 340,
//    LatencyCorrection = 0,
//    Approach = new Approach()
//    {
//        ProtectedPhaseNumber = 6,
//        DirectionTypeId = DirectionTypes.SB,
//        Mph = 45,
//        Signal = s
//    }
//};

var broadcast = new BroadcastBlock<IEnumerable<ControllerEventLog>>(null);

var ApproachDelayWorkflow = new ApproachDelayWorkflow();
ApproachDelayWorkflow.BeginInit();

//await foreach (var r in ApproachDelayWorkflow.Execute(list, default))
//{
//    Console.WriteLine($"result: {r}");
//}

var mergePlansAndDelayResults = new JoinBlock<ApproachDelayResult, IReadOnlyList<ApproachDelayPlan>>();

var ApproachDelayPlanResult = new ActionBlock<Tuple<ApproachDelayResult, IReadOnlyList<ApproachDelayPlan>>>(a =>
{


    //foreach (var r in a.Item1)
    //{
        var plans = a.Item2.ToList();

        foreach (var p in plans)
        {
            p.AssignToPlan(a.Item1.Vehicles);
        }

        a.Item1.Plans = plans.Where(w => w.Vehicles.Count > 0).ToList();

        Console.WriteLine($"result: {a.Item1} - {a.Item1.Plans.FirstOrDefault().PlanNumber}");
    //}


});


var FilteredPlanData = new FilteredPlanData();
var CalculateTimingPlans = new CalculateTimingPlans<ApproachDelayPlan>();

broadcast.LinkTo(ApproachDelayWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });
broadcast.LinkTo(FilteredPlanData, new DataflowLinkOptions() { PropagateCompletion = true });
FilteredPlanData.LinkTo(CalculateTimingPlans, new DataflowLinkOptions() { PropagateCompletion = true });

ApproachDelayWorkflow.Output.LinkTo(mergePlansAndDelayResults.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
CalculateTimingPlans.LinkTo(mergePlansAndDelayResults.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
mergePlansAndDelayResults.LinkTo(ApproachDelayPlanResult, new DataflowLinkOptions() { PropagateCompletion = true });

broadcast.Post(list);
broadcast.Complete();











//var CreateRedToRedCycles = new CreateRedToRedCycles();
//var result = await CreateRedToRedCycles.ExecuteAsync(list);

//foreach (var r in result)
//{
//    Console.WriteLine($"result: {r}");
//}


//var preemptionDetailsWorkflow = new PreemptionDetailsWorkflow();

//await foreach (var r in preemptionDetailsWorkflow.Execute(list.Take(20), default))
//{
//    Console.WriteLine($"result: {r}");

//    foreach (var delay in r.Delay)
//        Console.WriteLine($"delay: {delay.Seconds}");

//    foreach (var service in r.ServiceTimes)
//        Console.WriteLine($"service: {service.Seconds}");

//    foreach (var dwell in r.DwellTimes)
//        Console.WriteLine($"dwell: {dwell.Seconds}");

//    foreach (var max in r.CallMaxOutTimes)
//        Console.WriteLine($"max: {max.Seconds}");

//}














Console.ReadLine();


//public class VolumeByHour : TotalVolume
//{
//    //public int TotalVolume { get; set; }
//    public double PHF { get; set; }
//    public double DFactor { get; set; }
//    public double KFactor { get; set; }
//}






//var rootCmd = new EventLogCommands();
//var cmdBuilder = new CommandLineBuilder(rootCmd);
//cmdBuilder.UseDefaults();

//cmdBuilder.UseHost(a =>
//{
//    return Host.CreateDefaultBuilder(a)
//    .UseConsoleLifetime()
//    .ConfigureLogging((h, l) =>
//    {
//        //TODO: add a GoogleLogger section
//        //LoggingServiceOptions GoogleOptions = h.Configuration.GetSection("GoogleLogging").Get<LoggingServiceOptions>();
//        //TODO: remove this to an extension method
//        //DOTNET_ENVIRONMENT = Development,GOOGLE_APPLICATION_CREDENTIALS = M:\My Drive\ut-udot-atspm-dev-023438451801.json
//        if (h.Configuration.GetValue<bool>("UseGoogleLogger"))
//        {
//            l.AddGoogle(new LoggingServiceOptions
//            {
//                ProjectId = "1022556126938",
//                //ProjectId = "869261868126",
//                ServiceName = AppDomain.CurrentDomain.FriendlyName,
//                Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
//                Options = LoggingOptions.Create(LogLevel.Warning, AppDomain.CurrentDomain.FriendlyName)
//            });
//        }
//    })
//    .ConfigureServices((h, s) =>
//    {
//        s.AddLogging();

//        s.AddATSPMDbContext(h);

//        //repositories
//        s.AddScoped<ISignalRepository, SignalEFRepository>();
//        s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
//        //s.AddScoped<IControllerEventLogRepository, ControllerEventLogFileRepository>();

//        //s.AddTransient<IFileTranscoder, JsonFileTranscoder>();
//        //s.AddTransient<IFileTranscoder, ParquetFileTranscoder>();
//        s.AddTransient<IFileTranscoder, CompressedJsonFileTranscoder>();

//        //downloader clients
//        s.AddTransient<IHTTPDownloaderClient, HttpDownloaderClient>();
//        s.AddTransient<IFTPDownloaderClient, FluentFTPDownloaderClient>();
//        s.AddTransient<ISFTPDownloaderClient, SSHNetSFTPDownloaderClient>();

//        //downloaders
//        s.AddScoped<ISignalControllerDownloader, ASC3SignalControllerDownloader>();
//        s.AddScoped<ISignalControllerDownloader, CobaltSignalControllerDownloader>();
//        s.AddScoped<ISignalControllerDownloader, MaxTimeSignalControllerDownloader>();
//        s.AddScoped<ISignalControllerDownloader, EOSSignalControllerDownloader>();
//        s.AddScoped<ISignalControllerDownloader, NewCobaltSignalControllerDownloader>();

//        //decoders
//        s.AddScoped<ISignalControllerDecoder, ASCSignalControllerDecoder>();
//        s.AddScoped<ISignalControllerDecoder, MaxTimeSignalControllerDecoder>();

//        //SignalControllerLogger
//        //s.AddScoped<ISignalControllerLoggerService, CompressedSignalControllerLogger>();
//        s.AddScoped<ISignalControllerLoggerService, LegacySignalControllerLogger>();

//        //controller logger configuration
//        s.Configure<SignalControllerLoggerConfiguration>(h.Configuration.GetSection(nameof(SignalControllerLoggerConfiguration)));

//        //downloader configurations
//        s.ConfigureSignalControllerDownloaders(h);

//        //decoder configurations
//        s.ConfigureSignalControllerDecoders(h);

//        s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));

//        //command options
//        //if (cmd is ICommandOption<EventLogLoggingConfiguration> cmdOpt)
//        //{
//        //    s.AddSingleton(cmdOpt.GetOptionsBinder());
//        //    s.AddOptions<EventLogLoggingConfiguration>().BindCommandLine();

//        //    var opt = cmdOpt.GetOptionsBinder().CreateInstance(h.GetInvocationContext().BindingContext) as EventLogLoggingConfiguration;

//        //    //s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.LocalPath = opt.Path.FullName);
//        //    //s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.PingControllerToVerify = h.GetInvocationContext().ParseResult.GetValueForArgument(cmd.PingControllerArg));
//        //    //s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.DeleteFile = h.GetInvocationContext().ParseResult.GetValueForArgument(cmd.DeleteLocalFileArg));
//        //}

//        ////hosted services
//        //s.AddHostedService<SignalLoggerUtilityHostedService>();
//        //s.AddHostedService<TestSignalLoggerHostedService>();

//        //s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.LocalPath = s.configurall);
//    });
//},
//h =>
//{
//    var cmd = h.GetInvocationContext().ParseResult.CommandResult.Command;

//    h.ConfigureServices((h, s) =>
//    {
//        if (cmd is ICommandOption opt)
//        {
//            opt.BindCommandOptions(s);
//        }
//    });
//});

//var cmdParser = cmdBuilder.Build();
//await cmdParser.InvokeAsync(args);

public class TestExtractLogHostedService : IHostedService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<EventLogExtractConfiguration> _options;

    public TestExtractLogHostedService(ILogger<TestExtractLogHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogExtractConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StartAsync Cancelled..."));

        _serviceProvider.PrintHostInformation();

        _log.LogInformation("Extraction Path: {path}", _options.Value.Path);
        _log.LogInformation("Extraction File Formate: {format}", _options.Value.FileFormat);

        try
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                foreach (var s in _options.Value.Dates)
                {
                    _log.LogInformation("Extracting Event Logs for Date(s): {date}", s.ToString("dd/MM/yyyy"));
                }

                if (_options.Value.Included != null)
                {
                    foreach (var s in _options.Value.Included)
                    {
                        _log.LogInformation("Including Event Logs for Signal(s): {signal}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Signal(s): {signal}", s);
                    }
                }
            }
        }
        catch (Exception e)
        {

            _log.LogError("Exception: {e}", e);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StopAsync Cancelled..."));

        Console.WriteLine();
        Console.WriteLine($"Operation Completed or Cancelled...");

        return Task.CompletedTask;
    }
}

public class TestSignalLoggerHostedService : IHostedService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<EventLogLoggingConfiguration> _options;

    public TestSignalLoggerHostedService(ILogger<TestSignalLoggerHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogLoggingConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StartAsync Cancelled..."));

        _serviceProvider.PrintHostInformation();

        _log.LogInformation("Extraction Path: {path}", _options.Value.Path);

        try
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                foreach (var option in scope.ServiceProvider.GetServices<IOptionsSnapshot<SignalControllerDownloaderConfiguration>>())
                {
                    Console.WriteLine($"------------local path: {option.Value.LocalPath}");
                    Console.WriteLine($"------------ping: {option.Value.PingControllerToVerify}");
                    Console.WriteLine($"------------delete: {option.Value.DeleteFile}");
                }

                if (_options.Value.ControllerTypes != null)
                {
                    foreach (var s in _options.Value.ControllerTypes)
                    {
                        _log.LogInformation("Including Event Logs for Types(s): {type}", s);
                    }
                }

                if (_options.Value.Included != null)
                {
                    foreach (var s in _options.Value.Included)
                    {
                        _log.LogInformation("Including Event Logs for Signal(s): {signal}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Signal(s): {signal}", s);
                    }
                }
            }
        }
        catch (Exception e)
        {

            _log.LogError("Exception: {e}", e);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StopAsync Cancelled..."));

        Console.WriteLine();
        Console.WriteLine($"Operation Completed or Cancelled...");

        return Task.CompletedTask;
    }
}

public class TestSignalInfoHostedService : IHostedService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<EventLogSignalInfoConfiguration> _options;

    public TestSignalInfoHostedService(ILogger<TestSignalInfoHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogSignalInfoConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StartAsync Cancelled..."));

        _serviceProvider.PrintHostInformation();

        try
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {

                if (_options.Value.ControllerTypes != null)
                {
                    foreach (var s in _options.Value.ControllerTypes)
                    {
                        _log.LogInformation("Including Event Logs for Types(s): {type}", s);
                    }
                }

                if (_options.Value.Included != null)
                {
                    foreach (var s in _options.Value.Included)
                    {
                        _log.LogInformation("Including Event Logs for Signal(s): {signal}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Signal(s): {signal}", s);
                    }
                }
            }
        }
        catch (Exception e)
        {

            _log.LogError("Exception: {e}", e);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StopAsync Cancelled..."));

        Console.WriteLine();
        Console.WriteLine($"Operation Completed or Cancelled...");

        return Task.CompletedTask;
    }
}

public class TestAggregationHostedService : IHostedService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<EventLogAggregateConfiguration> _options;

    public TestAggregationHostedService(ILogger<TestAggregationHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogAggregateConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StartAsync Cancelled..."));

        _serviceProvider.PrintHostInformation();

        try
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                Console.WriteLine($"------------type: {_options.Value.AggregationType}");
                Console.WriteLine($"------------size: {_options.Value.BinSize}");

                if (_options.Value.Dates != null)
                {
                    foreach (var s in _options.Value.Dates)
                    {
                        _log.LogInformation("Extracting Event Logs for Date(s): {date}", s.ToString("dd/MM/yyyy"));
                    }
                }

                if (_options.Value.Included != null)
                {
                    foreach (var s in _options.Value.Included)
                    {
                        _log.LogInformation("Including Event Logs for Signal(s): {signal}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Signal(s): {signal}", s);
                    }
                }
            }
        }
        catch (Exception e)
        {

            _log.LogError("Exception: {e}", e);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StopAsync Cancelled..."));

        Console.WriteLine();
        Console.WriteLine($"Operation Completed or Cancelled...");

        return Task.CompletedTask;
    }
}
