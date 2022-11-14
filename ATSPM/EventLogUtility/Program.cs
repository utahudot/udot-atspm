using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Application.Services;
using ATSPM.Data;
using ATSPM.Domain.Common;
using ATSPM.EventLogUtility;
using ATSPM.EventLogUtility.Commands;
using ATSPM.Infrastructure.Converters;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.SignalControllerLoggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Security.Cryptography;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Runtime.CompilerServices;


var rootCmd = new EventLogCommands();
var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();

cmdBuilder.UseHost(a => Host.CreateDefaultBuilder(a).UseConsoleLifetime(), h =>
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
await cmdParser.InvokeAsync("log -d 10/10/2020 -i 1001 1002");

public static class CommandHostBuilder
{
    public static void BuildSignalLoggerHost(this IHostBuilder hostBuilder, LogConsoleCommand cmd)
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

            ////downloader clients
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
            }

            //hosted services
            //s.AddHostedService<LoggerBackgroundService>();
        });
    }

    public static void BuildExtractLogHost(this IHostBuilder hostBuilder, ExtractConsoleCommand cmd)
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
        });
    }
}

