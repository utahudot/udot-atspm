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
using ATSPM.Application.Analysis.Workflows;
using System.Security.Cryptography.X509Certificates;


//var path1 = "C:\\temp\\TestData\\7115_Approach_Delay.csv";
//var path2 = "C:\\temp\\TestData\\1001_Approach_Delay.csv";

var path1 = "C:\\temp\\TestData\\4020_6-15-2023_PreemptionData.csv";

var list = File.ReadAllLines(path1)
               .Skip(1)
               .Select(x => x.Split(','))
               .Select(x => new ControllerEventLog
               {
                   LocationIdentifier = x[0],
                   Timestamp = DateTime.Parse(x[1]),
                   EventCode = int.Parse(x[2]),
                   EventParam = int.Parse(x[3])
               }).ToList();


//var path2 = "C:\\temp\\TestData\\7115_4-17-2023_Plans.csv";

//var list2 = File.ReadAllLines(path2)
//               .Skip(1)
//               .Select(x => x.Split(','))
//               .Select(x => new ControllerEventLog
//               {
//                   LocationId = x[0],
//                   Timestamp = DateTime.Parse(x[1]),
//                   EventCode = int.Parse(x[2]),
//                   EventParam = int.Parse(x[3])
//               }).ToList();

//Console.WriteLine($"list2: {list2.Count}");

//list = list.Union(list2).ToList();

//Console.WriteLine($"list2: {list2.Where(w => w.EventCode == 131).Count()}");


//var s = new Location() { LocationId = "7191" };

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
//        Location = s
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
//        Location = s
//    }
//};

var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 111, EventParam = 1},

                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 7:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 7:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 7:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 7:04:01.1"), EventCode = 111, EventParam = 1},

                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 5:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 5:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 5:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 5:04:01.1"), EventCode = 111, EventParam = 1},
            };


PreemptiveStuff test = new PreemptiveStuff();

var result = await test.ExecuteAsync(testData);


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
//        s.AddScoped<ILocationRepository, LocationEFRepository>();
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
//        s.AddScoped<IDeviceDownloader, ASC3SignalControllerDownloader>();
//        s.AddScoped<IDeviceDownloader, CobaltLocationControllerDownloader>();
//        s.AddScoped<IDeviceDownloader, MaxTimeLocationControllerDownloader>();
//        s.AddScoped<IDeviceDownloader, EOSSignalControllerDownloader>();
//        s.AddScoped<IDeviceDownloader, NewCobaltLocationControllerDownloader>();

//        //decoders
//        s.AddScoped<ILocationControllerDecoder, ASCLocationControllerDecoder>();
//        s.AddScoped<ILocationControllerDecoder, MaxTimeLocationControllerDecoder>();

//        //LocationControllerLogger
//        //s.AddScoped<ILocationControllerLoggerService, CompressedLocationControllerLogger>();
//        s.AddScoped<ILocationControllerLoggerService, LegacyLocationControllerLogger>();

//        //controller logger configuration
//        s.Configure<LocationControllerLoggerConfiguration>(h.Configuration.GetSection(nameof(LocationControllerLoggerConfiguration)));

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
//        //s.AddHostedService<LocationLoggerUtilityHostedService>();
//        //s.AddHostedService<TestLocationLoggerHostedService>();

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
                        _log.LogInformation("Including Event Logs for Location(s): {Location}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Location(s): {Location}", s);
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

public class TestLocationLoggerHostedService : IHostedService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<EventLogLoggingConfiguration> _options;

    public TestLocationLoggerHostedService(ILogger<TestLocationLoggerHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogLoggingConfiguration> options) =>
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
                        _log.LogInformation("Including Event Logs for Location(s): {Location}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Location(s): {Location}", s);
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

public class TestLocationInfoHostedService : IHostedService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<EventLogLocationInfoConfiguration> _options;

    public TestLocationInfoHostedService(ILogger<TestLocationInfoHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogLocationInfoConfiguration> options) =>
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
                        _log.LogInformation("Including Event Logs for Location(s): {Location}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Location(s): {Location}", s);
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
                        _log.LogInformation("Including Event Logs for Location(s): {Location}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Location(s): {Location}", s);
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
