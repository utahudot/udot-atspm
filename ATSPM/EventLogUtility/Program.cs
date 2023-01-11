using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Data;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using ATSPM.EventLogUtility.Commands;
using ATSPM.Infrastructure.Converters;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.HostedServices;
using ATSPM.Infrastructure.Services.SignalControllerLoggers;
using Google.Api;
using Google.Cloud.Diagnostics.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Reflection;

var rootCmd = new EventLogCommands();
var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();

cmdBuilder.UseHost(a =>
{
    return Host.CreateDefaultBuilder(a)
    .UseConsoleLifetime()
    .ConfigureLogging((h, l) =>
    {
        //TODO: add a GoogleLogger section
        //LoggingServiceOptions GoogleOptions = h.Configuration.GetSection("GoogleLogging").Get<LoggingServiceOptions>();
        //TODO: remove this to an extension method
        //DOTNET_ENVIRONMENT = Development,GOOGLE_APPLICATION_CREDENTIALS = M:\My Drive\ut-udot-atspm-dev-023438451801.json
        if (h.Configuration.GetValue<bool>("UseGoogleLogger"))
        {
            Console.WriteLine("hello");
            l.AddGoogle(new LoggingServiceOptions
            {
                ProjectId = "1022556126938",
                //ProjectId = "869261868126",
                ServiceName = AppDomain.CurrentDomain.FriendlyName,
                Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                Options = LoggingOptions.Create(LogLevel.Information, AppDomain.CurrentDomain.FriendlyName)
            });
        }
    });
}, 
h =>
{
    var cmd = h.GetInvocationContext().ParseResult.CommandResult.Command;

    switch (cmd)
    {
        case LogConsoleCommand:
            {
                h.BuildSignalLoggerHost((LogConsoleCommand)cmd);
                break;
            }
        case ExtractConsoleCommand:
            {
                h.BuildExtractLogHost((ExtractConsoleCommand)cmd);
                break;
            }
    }
});

var cmdParser = cmdBuilder.Build();
await cmdParser.InvokeAsync("log -t 4 -i 1014 1015 1016 1023");

public static class CommandHostBuilder
{
    public static IHostBuilder BuildSignalLoggerHost(this IHostBuilder hostBuilder, LogConsoleCommand cmd)
    {
        hostBuilder.ConfigureServices((h, s) =>
        {
            s.AddLogging();

            s.AddATSPMDbContext(h);

            //repositories
            s.AddScoped<ISignalRepository, SignalEFRepository>();
            //s.AddScoped<ISignalRepository, SignalFileRepository>();
            s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
            //s.AddScoped<IControllerEventLogRepository, ControllerEventLogFileRepository>();

            //s.AddTransient<IFileTranscoder, JsonFileTranscoder>();
            //s.AddTransient<IFileTranscoder, ParquetFileTranscoder>();
            s.AddTransient<IFileTranscoder, CompressedJsonFileTranscoder>();

            //downloader clients
            s.AddTransient<IHTTPDownloaderClient, HttpDownloaderClient>();
            s.AddTransient<IFTPDownloaderClient, FluentFTPDownloaderClient>();
            s.AddTransient<ISFTPDownloaderClient, SSHNetSFTPDownloaderClient>();

            //downloaders
            s.AddScoped<ISignalControllerDownloader, ASC3SignalControllerDownloader>();
            s.AddScoped<ISignalControllerDownloader, CobaltSignalControllerDownloader>();
            s.AddScoped<ISignalControllerDownloader, MaxTimeSignalControllerDownloader>();
            s.AddScoped<ISignalControllerDownloader, EOSSignalControllerDownloader>();
            s.AddScoped<ISignalControllerDownloader, NewCobaltSignalControllerDownloader>();

            //decoders
            s.AddScoped<ISignalControllerDecoder, ASCSignalControllerDecoder>();
            s.AddScoped<ISignalControllerDecoder, MaxTimeSignalControllerDecoder>();

            //SignalControllerDataFlow
            //s.AddScoped<ISignalControllerLoggerService, CompressedSignalControllerLogger>();
            s.AddScoped<ISignalControllerLoggerService, LegacySignalControllerLogger>();

            //controller logger configuration
            s.Configure<SignalControllerLoggerConfiguration>(h.Configuration.GetSection(nameof(SignalControllerLoggerConfiguration)));

            //downloader configurations
            s.Configure<SignalControllerDownloaderConfiguration>(nameof(ASC3SignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(ASC3SignalControllerDownloader)}"));
            s.Configure<SignalControllerDownloaderConfiguration>(nameof(CobaltSignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(CobaltSignalControllerDownloader)}"));
            s.Configure<SignalControllerDownloaderConfiguration>(nameof(MaxTimeSignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(MaxTimeSignalControllerDownloader)}"));
            s.Configure<SignalControllerDownloaderConfiguration>(nameof(EOSSignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(EOSSignalControllerDownloader)}"));
            s.Configure<SignalControllerDownloaderConfiguration>(nameof(NewCobaltSignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(NewCobaltSignalControllerDownloader)}"));

            //decoder configurations
            s.Configure<SignalControllerDecoderConfiguration>(nameof(ASCSignalControllerDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(ASCSignalControllerDecoder)}"));
            s.Configure<SignalControllerDecoderConfiguration>(nameof(MaxTimeSignalControllerDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(MaxTimeSignalControllerDecoder)}"));

            s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));

            //command options
            if (cmd is ICommandOption<EventLogLoggingConfiguration> cmdOpt)
            {
                s.AddSingleton(cmdOpt.GetOptionsBinder());
                s.AddOptions<EventLogLoggingConfiguration>().BindCommandLine();

                var opt = cmdOpt.GetOptionsBinder().CreateInstance(h.GetInvocationContext().BindingContext) as EventLogLoggingConfiguration;

                s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.LocalPath = opt.Path.FullName);
                s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.PingControllerToVerify = h.GetInvocationContext().ParseResult.GetValueForArgument(cmd.PingControllerArg));
                s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.DeleteFile = h.GetInvocationContext().ParseResult.GetValueForArgument(cmd.DeleteLocalFileArg));
            }

            //hosted services
            s.AddHostedService<SignalLoggerUtilityHostedService>();
            //s.AddHostedService<TestSignalLoggerHostedService>();

            //s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.LocalPath = s.configurall);
        });

        return hostBuilder;
    }

    public static IHostBuilder BuildExtractLogHost(this IHostBuilder hostBuilder, ExtractConsoleCommand cmd)
    {
        hostBuilder.ConfigureServices((h, s) =>
        {
            //databases
            s.AddDbContext<EventLogContext>(db => db.UseSqlServer(h.Configuration.GetConnectionString(nameof(EventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(h.HostingEnvironment.IsDevelopment()));

            //repositories
            s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();

            //command options
            if (cmd is ICommandOption<EventLogExtractConfiguration> cmdOpt)
            {
                s.AddSingleton(cmdOpt.GetOptionsBinder());
                s.AddOptions<EventLogLoggingConfiguration>().BindCommandLine();
            }

            //hosted services
            //s.AddHostedService<ExportUtilityService>();
            s.AddHostedService<TestExtractLogHostedService>();
        });

        return hostBuilder;
    }
}

public class TestExtractLogHostedService : IHostedService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<EventLogExtractConfiguration> _options;

    public TestExtractLogHostedService(ILogger<TestExtractLogHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogExtractConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

    public Task StartAsync(CancellationToken cancellationToken)
    {
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
        return Task.CompletedTask;
    }
}

