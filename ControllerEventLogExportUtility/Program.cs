using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Data;
using ATSPM.Domain.Common;
using ATSPM.Infrastructure.Converters;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.SignalControllerLoggers;
using ControllerEventLogExportUtility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.CommandLine.Invocation;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Options;
using System;


var extractCmd = new ExtractConsoleCommand();
var rootCmd = new RootCommand();
rootCmd.AddCommand(extractCmd);


//rootCmd.Invoke(args);



//    .UseHost


//var host = Host.CreateDefaultBuilder()

var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();



cmdBuilder.UseHost(a => Host.CreateDefaultBuilder(), h =>
{
    //var h = Host.CreateDefaultBuilder()
    h.ConfigureAppConfiguration((h, c) =>
    {
        //c.AddUserSecrets("af468330-96e6-4297-a188-86f216ee07b4");
        //c.AddCommandLine(args);
        //c.AddCommandLineDirectives(result, "name");
    })
    .ConfigureLogging((h, l) =>
    {
        //l.SetMinimumLevel(LogLevel.None);

        //TODO: add a GoogleLogger section
        //LoggingServiceOptions GoogleOptions = h.Configuration.GetSection("GoogleLogging").Get<LoggingServiceOptions>();
        //TODO: remove this to an extension method
        //DOTNET_ENVIRONMENT = Development,GOOGLE_APPLICATION_CREDENTIALS = M:\My Drive\ut-udot-atspm-dev-023438451801.json
        //if (h.Configuration.GetValue<bool>("UseGoogleLogger"))
        //{
        //    l.AddGoogle(new LoggingServiceOptions
        //    {
        //        ProjectId = "1022556126938",
        //        //ProjectId = "869261868126",
        //        ServiceName = AppDomain.CurrentDomain.FriendlyName,
        //        Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
        //        Options = LoggingOptions.Create(LogLevel.Information, AppDomain.CurrentDomain.FriendlyName)
        //    });
        //}
    })
    .ConfigureServices((h, s) =>
    {
        //s.AddGoogleErrorReporting(new ErrorReportingServiceOptions() {
        //    ProjectId = "1022556126938",
        //    ServiceName = "ErrorReporting",
        //    Version = "1.1",
        //});

        //s.AddLogging();
        s.AddDbContext<EventLogContext>(db => db.UseSqlServer(h.Configuration.GetConnectionString(nameof(EventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(h.HostingEnvironment.IsDevelopment()));

        //s.AddATSPMDbContext(h);

        //background services
        //s.AddHostedService<ExportUtilityService>();
        s.AddHostedService<TestWorker>();

        //repositories
        s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();


        //s.Configure<TestConfig>(a => a.Test = h.Configuration.GetValue<int>("sub1:Test"));
        //s.Configure<ExtractConsoleConfiguration>(h.Configuration.GetSection("extract"));
    });

    //.UseConsoleLifetime();

});
    
var parser = cmdBuilder.Build();
await parser.InvokeAsync(args);

////var result = parser.Build().Parse(args);

//hello.Invoke(args);

//var config = hello.Invoke().GetService<IConfiguration>();

//foreach (var p in config.AsEnumerable())
//{
//    Console.Write($"key-{p.Key} value-{p.Value}\n");
//}




//await host.RunAsync();

//Console.ReadLine();
//Console.ReadKey();

public class TestWorker : BackgroundService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<ExtractConsoleConfiguration> _options;
    public TestWorker(ILogger<TestWorker> log, IServiceProvider serviceProvider, IOptions<ExtractConsoleConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"Extraction path {_options.Value.Path}");

        try
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var eventRepository = scope.ServiceProvider.GetService<IControllerEventLogRepository>();

                foreach (var s in _options.Value.Dates)
                {
                    Console.WriteLine($"Extracting event logs for {s:dd/MM/yyyy}");
                }

                var archiveQuery = eventRepository.GetList().Where(i => _options.Value.Dates.Any(d => i.ArchiveDate == d));

                //if (_options.Value.Included != null)
                //{
                //    foreach (var s in _options.Value.Included)
                //    {
                //        Console.WriteLine($"Extracting event logs for signal {s}");
                //    }

                //    archiveQuery = archiveQuery.Where(i => _options.Value.Included.Any(d => i.SignalId == d));
                //}

                //if (_options.Value.Excluded != null)
                //{
                //    foreach (var s in _options.Value.Excluded)
                //    {
                //        Console.WriteLine($"Excluding event logs for signal {s}");
                //    }

                //    archiveQuery = archiveQuery.Where(i => !_options.Value.Excluded.Contains(i.SignalId));
                //}

                int skip = 0;
                int take = 20;

                int count = archiveQuery.Count();
                Console.WriteLine($"archiveQuery Count: {count}");

                int newCount = 0;

                while (skip < count)
                {
                    foreach (var a in await archiveQuery.Skip(skip).Take(take).ToListAsync())
                    {
                        Console.WriteLine($"Returned Archive: {a}");

                        newCount++;
                    }

                    skip = skip + take;
                }

                Console.WriteLine($"newCount: {newCount}");


                //if (_options.Value.ControllerTypes != null)
                //{
                //    archiveQuery = archiveQuery.Where(i => _options.Value.ControllerTypes.Any(d => i == d));
                //}

                //Console.WriteLine($"Archive count {count}");

                //var config = scope.ServiceProvider.GetService<IConfiguration>();

                //foreach (var c in config.AsEnumerable())
                //{
                //    Console.WriteLine($"config {c.Key} : {c.Value}"); 
                //}

            }
        }
        catch (Exception e)
        {

            _log.LogError("Exception: {e}", e);
        }


        //_serviceProvider?.GetService<IHostApplicationLifetime>()?.StopApplication();
        //return Task.CompletedTask;
    }
}


public class ExtractConsoleConfiguration
{
    public IEnumerable<DateTime> Dates { get; set; }
    public IEnumerable<string> Included { get; set; }
    public IEnumerable<string> Excluded { get; set; }
    public IEnumerable<int> ControllerTypes { get; set; }
    public DirectoryInfo Path { get; set; }
}

public static class Testing
{
    private const string ConfigurationDirectiveName = "config";

    public static CommandLineBuilder UseHost(this CommandLineBuilder builder,
            Func<string[], IHostBuilder> hostBuilderFactory,
            Action<IHostBuilder> configureHost = null) =>
            builder.AddMiddleware(async (invocation, next) =>
            {
                var argsRemaining = invocation.ParseResult.UnparsedTokens.ToArray();
                var hostBuilder = hostBuilderFactory?.Invoke(argsRemaining)
                    ?? new HostBuilder();
                hostBuilder.Properties[typeof(InvocationContext)] = invocation;

                hostBuilder.ConfigureHostConfiguration(config =>
                {
                    config.AddCommandLineDirectives(invocation.ParseResult, ConfigurationDirectiveName);


                    //config.AddCommandLineConfig(invocation);

                });
                hostBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton(invocation);
                    services.AddSingleton(invocation.BindingContext);
                    services.AddSingleton(invocation.Console);
                    services.AddTransient(_ => invocation.InvocationResult);
                    services.AddTransient(_ => invocation.ParseResult);

                    if (invocation.ParseResult.CommandResult.Command is ExtractConsoleCommand cmd)
                    {
                        //var config = cmd.ParseOptions(invocation);
                        services.PostConfigure<ExtractConsoleConfiguration>(c => cmd.ParseOptions(c, invocation));
                    }
                });
                hostBuilder.UseInvocationLifetime(invocation);
                configureHost?.Invoke(hostBuilder);

                using var host = hostBuilder.Build();

                invocation.BindingContext.AddService(typeof(IHost), _ => host);

                //await host.RunAsync();

                await host.StartAsync();
                //await next(invocation);
                await host.StopAsync();
            });

}

public static class DirectiveConfigurationExtensions
{
    public static IConfigurationBuilder AddCommandLineDirectives(
        this IConfigurationBuilder config, ParseResult commandline,
        string name)
    {
        if (commandline is null)
            throw new ArgumentNullException(nameof(commandline));
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        if (!commandline.Directives.TryGetValues(name, out var directives))
            return config;

        var kvpSeparator = new[] { '=' };
        return config.AddInMemoryCollection(directives.Select(s =>
        {
            var parts = s.Split(kvpSeparator, count: 2);
            var key = parts[0];
            var value = parts.Length > 1 ? parts[1] : null;
            return new KeyValuePair<string, string>(key, value);
        }).ToList());
    }

    //public static IConfigurationBuilder AddCommandLineConfig(this IConfigurationBuilder config, InvocationContext context)
    //{
    //    var group = context.ParseResult.CommandResult.Command.Name;

    //    Console.WriteLine($"group name: {group}");

    //    var dict = new Dictionary<string, string?>();

    //    foreach (var child in context.ParseResult.CommandResult.Children)
    //    {
    //        if (child is OptionResult r)
    //        {
    //            Console.WriteLine($"result: {r.Option.Name}:{r.GetValueOrDefault()}");

    //            dict.Add($"{group}:{r.Option.Name}", r.GetValueOrDefault()?.ToString());
    //        }
    //    }

    //    return config.AddInMemoryCollection(dict.AsEnumerable());
    //}
}