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


//var path1 = "C:\\temp\\TestData\\7115_Approach_Delay.csv";
//var path2 = "C:\\temp\\TestData\\1001_Approach_Delay.csv";

var path1 = "C:\\temp\\TestData\\7115_Approach_Volume.csv";

var list =  File.ReadAllLines(path1)
               .Skip(1)
               .Select(x => x.Split(','))
               .Select(x => new ControllerEventLog
               {
                   SignalId = x[0],
                   Timestamp = DateTime.Parse(x[1]),
                   EventCode = int.Parse(x[2]),
                   EventParam = int.Parse(x[3])
               }).ToList();



var s = new Signal() { SignalId = "7115" };


var d1 = new Detector()
{
    DetChannel = 2,
    DistanceFromStopBar = 340,
    LatencyCorrection = 1.2,
    Approach = new Approach()
    {
        ProtectedPhaseNumber = 2,
        DirectionTypeId = DirectionTypes.NB,
        Mph = 45,
        Signal = s
    }
};

var d2 = new Detector()
{
    DetChannel = 6,
    DistanceFromStopBar = 340,
    LatencyCorrection = 1.2,
    Approach = new Approach()
    {
        ProtectedPhaseNumber = 6,
        DirectionTypeId = DirectionTypes.SB,
        Mph = 45,
        Signal = s
    }
};

int bin = 15;

var start = DateTime.Parse("4/17/2023 8:00:0.0");
var end = DateTime.Parse("4/17/2023 10:00:0.0");


var timeFrame = DateTimeRange.GenerateTimeFrameInMinutes(start, end, bin);

foreach (var t in timeFrame)
{
    Console.WriteLine($"T: {t}");
}

Console.WriteLine($"input count: {list.Count}");

var config = new List<Tuple<Detector, IEnumerable<ControllerEventLog>>>()
{
    Tuple.Create(d1, list.Where(l => l.EventParam == d1.DetChannel)),
    Tuple.Create(d2, list.Where(l => l.EventParam == d2.DetChannel))
};

var identifyandAdjustVehicleActivations = new IdentifyandAdjustVehicleActivations();

var correctedDetectorEvents = await identifyandAdjustVehicleActivations.ExecuteAsync(config);

Console.WriteLine($"corrected count: {correctedDetectorEvents.Count()}");


//get phase 2 events in 15 min chunks
var e = correctedDetectorEvents.GroupBy(g => g.Phase, (k, v) =>
    timeFrame
    .Select((s, i) => new Volume()
    {
        Phase = k,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        DetectorCount = v.Where(w => w.TimeStamp >= s.StartTime && w.TimeStamp < s.EndTime).Count()
    })).SelectMany(m => m.Where(v => v != null));

foreach (var v in e)
{
    Console.WriteLine($"v: {v}");
}














Console.ReadLine();

public interface IDateTimeRange
{
    DateTime StartTime { get; set; }

    DateTime EndTime { get; set; }

    bool InRange(DateTime time);
}

public class DateTimeRange : IDateTimeRange
{
    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool InRange(DateTime time)
    {
        return time >= StartTime && time < EndTime;
    }

    public static List<DateTimeRange> GenerateTimeFrameInHours(DateTime start, DateTime end, int size)
    {
        var values = Enumerable
            .Range(0, Convert.ToInt32((end.TimeOfDay.TotalHours - start.TimeOfDay.TotalHours) / size))
            .Select((s, i) => start.AddHours(i * size)).ToList();

        return ReturnDateTimeRangeList(values);
    }

    public static List<DateTimeRange> GenerateTimeFrameInMinutes(DateTime start, DateTime end, int size)
    {
        var values = Enumerable
            .Range(0, Convert.ToInt32((end.TimeOfDay.TotalMinutes - start.TimeOfDay.TotalMinutes) / size))
            .Select((s, i) => start.AddMinutes(i * size)).ToList();

        return ReturnDateTimeRangeList(values);
    }

    public static List<DateTimeRange> GenerateTimeFrameInSeconds(DateTime start, DateTime end, int size)
    {
        var values = Enumerable
            .Range(0, Convert.ToInt32((end.TimeOfDay.TotalSeconds - start.TimeOfDay.TotalSeconds) / size))
            .Select((s, i) => start.AddSeconds(i * size)).ToList();

        return ReturnDateTimeRangeList(values);
    }

    private static List<DateTimeRange> ReturnDateTimeRangeList(List<DateTime> values)
    {
        return values.Take(values.Count() - 1).Select((s, i) => new DateTimeRange() { StartTime = values[0], EndTime = values[0 + 1] }).ToList();
    }
}







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
