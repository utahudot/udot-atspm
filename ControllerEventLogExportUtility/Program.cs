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



var extractionDateOption = new Option<IEnumerable<DateTime>>("--date", () => new List<DateTime>() { DateTime.Now }, "Date to extract event logs for in dd/mm/yyyy format")
{
    IsRequired = true,
    AllowMultipleArgumentsPerToken = true
};
extractionDateOption.AddAlias("-d");
extractionDateOption.AddValidator(r =>
{
    if (r.GetValueForOption(extractionDateOption).Any(a => a > DateTime.Now))
        r.ErrorMessage = "Date must not be greater than current date";
});

var extractionIncludeOption = new Option<IEnumerable<string>>("--include", "List of signal controller numbers to include")
{
    AllowMultipleArgumentsPerToken = true
};
extractionIncludeOption.AddAlias("-i");

var extractionExcludeOption = new Option<IEnumerable<string>>("--exclude", "List of signal controller numbers to exclude")
{
    AllowMultipleArgumentsPerToken = true
};
extractionExcludeOption.AddAlias("-e");

extractionIncludeOption.AddValidator(r =>
{
    if (r.GetValueForOption(extractionExcludeOption)?.Count() > 0)
        r.ErrorMessage = "Can't use include option when also using exclude option";
});
extractionExcludeOption.AddValidator(r =>
{
    if (r.GetValueForOption(extractionIncludeOption)?.Count() > 0)
        r.ErrorMessage = "Can't use exclude option when also using include option";
});

var extractionTypeOption = new Option<IEnumerable<int>>("--type", "List of controller types to extract")
{
    AllowMultipleArgumentsPerToken = true
};
extractionTypeOption.AddAlias("-t");

var extractionPathOption = new Option<DirectoryInfo>("--path", () => new DirectoryInfo(Path.Combine("C:", "temp", "exports")), "Path to extraction directory");
extractionPathOption.AddAlias("-p");

var extractCmd = new Command("extract", "Extract compressed controller event logs");
extractCmd.AddOption(extractionDateOption);
extractCmd.AddOption(extractionIncludeOption);
extractCmd.AddOption(extractionExcludeOption);
extractCmd.AddOption(extractionTypeOption);
extractCmd.AddOption(extractionPathOption);

extractCmd.SetHandler((d, i, e, t, p) =>
{
    foreach (var s in d)
    {
        Console.WriteLine($"Extracting event logs for {s:dd/MM/yyyy}");
    }

    foreach (var s in i)
    {
        Console.WriteLine($"Extracting event logs for signal {s}");
    }

    foreach (var s in e)
    {
        Console.WriteLine($"Excluding event logs for signal {s}");
    }

    foreach (var s in t)
    {
        Console.WriteLine($"Extracting event logs for signal type {t}");
    }

    Console.WriteLine($"Extraction path {p}");

}, extractionDateOption, extractionIncludeOption, extractionExcludeOption, extractionTypeOption, extractionPathOption);


var rootCmd = new RootCommand();
rootCmd.AddCommand(extractCmd);




rootCmd.Invoke(args);



//    .UseHost


//var host = Host.CreateDefaultBuilder()

//var parser = new CommandLineBuilder(rootCommand);
//parser.UseDefaults();

////parser.AddMiddleware((c, n) =>
////{
////    var group = c.ParseResult.CommandResult.Command.Name;

////    Console.WriteLine($"group name: {group}");

////    foreach (var child in c.ParseResult.CommandResult.Children)
////    {

////    }

////    return n(c);
////});


//var hello = parser.UseHost(a => new HostBuilder(), h =>
//{
//    //var h = Host.CreateDefaultBuilder()
//    h.ConfigureAppConfiguration((h, c) =>
//    {
//        //c.AddUserSecrets("af468330-96e6-4297-a188-86f216ee07b4");
//        //c.AddCommandLine(args);
//        //c.AddCommandLineDirectives(result, "name");
//    })
//    .ConfigureLogging((h, l) =>
//    {
//        //l.SetMinimumLevel(LogLevel.None);

//        //TODO: add a GoogleLogger section
//        //LoggingServiceOptions GoogleOptions = h.Configuration.GetSection("GoogleLogging").Get<LoggingServiceOptions>();
//        //TODO: remove this to an extension method
//        //DOTNET_ENVIRONMENT = Development,GOOGLE_APPLICATION_CREDENTIALS = M:\My Drive\ut-udot-atspm-dev-023438451801.json
//        //if (h.Configuration.GetValue<bool>("UseGoogleLogger"))
//        //{
//        //    l.AddGoogle(new LoggingServiceOptions
//        //    {
//        //        ProjectId = "1022556126938",
//        //        //ProjectId = "869261868126",
//        //        ServiceName = AppDomain.CurrentDomain.FriendlyName,
//        //        Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
//        //        Options = LoggingOptions.Create(LogLevel.Information, AppDomain.CurrentDomain.FriendlyName)
//        //    });
//        //}
//    })
//    .ConfigureServices((h, s) =>
//    {
//        //s.AddGoogleErrorReporting(new ErrorReportingServiceOptions() {
//        //    ProjectId = "1022556126938",
//        //    ServiceName = "ErrorReporting",
//        //    Version = "1.1",
//        //});

//        //s.AddLogging();
//        s.AddDbContext<EventLogContext>(db => db.UseSqlServer(h.Configuration.GetConnectionString(nameof(EventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(h.HostingEnvironment.IsDevelopment()));

//        //s.AddATSPMDbContext(h);

//        //background services
//        //s.AddHostedService<ExportUtilityService>();
//        s.AddHostedService<TestWorker>();

//        //repositories
//        s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();


//        //s.Configure<TestConfig>(a => a.Test = h.Configuration.GetValue<int>("sub1:Test"));
//        s.Configure<TestConfig>(h.Configuration.GetSection("sub1"));
//    });

//    //.UseConsoleLifetime();

//}).Build();

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
    private IOptions<TestConfig> _options;
    public TestWorker(IOptions<TestConfig> options)
    {
        _options = options;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"Test Worker: {_options.Value.Test}");

        return Task.CompletedTask;
    }
}


public class TestConfig
{
    public int Test { get; set; }
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


                    config.AddCommandLineConfig(invocation);

                });
                hostBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton(invocation);
                    services.AddSingleton(invocation.BindingContext);
                    services.AddSingleton(invocation.Console);
                    services.AddTransient(_ => invocation.InvocationResult);
                    services.AddTransient(_ => invocation.ParseResult);
                });
                hostBuilder.UseInvocationLifetime(invocation);
                configureHost?.Invoke(hostBuilder);

                using var host = hostBuilder.Build();

                invocation.BindingContext.AddService(typeof(IHost), _ => host);

                await host.StartAsync();

                await next(invocation);

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

    public static IConfigurationBuilder AddCommandLineConfig(this IConfigurationBuilder config, InvocationContext context)
    {
        var group = context.ParseResult.CommandResult.Command.Name;

        //Console.WriteLine($"group name: {group}");

        var dict = new Dictionary<string, string?>();

        foreach (var child in context.ParseResult.CommandResult.Children)
        {
            if (child is OptionResult r)
            {
                //Console.WriteLine($"result: {r.Option.Name}:{r.GetValueOrDefault()}");

                dict.Add($"{group}:{r.Option.Name}", r.GetValueOrDefault()?.ToString());
            }
        }

        return config.AddInMemoryCollection(dict.AsEnumerable());
    }
}