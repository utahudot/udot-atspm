using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.EventLogUtility;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

var extractCmd = new ExtractConsoleCommand();
var rootCmd = new RootCommand();
rootCmd.AddCommand(extractCmd);

var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();

cmdBuilder.UseHost(a => Host.CreateDefaultBuilder(), h =>
{
    h.ConfigureServices((h, s) =>
    {
        //databases
        s.AddDbContext<EventLogContext>(db => db.UseSqlServer(h.Configuration.GetConnectionString(nameof(EventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(h.HostingEnvironment.IsDevelopment()));

        //repositories
        s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();

        //Hosted services
        s.AddHostedService<ExportUtilityService>();
    })
    .UseConsoleLifetime();

});

var cmdParser = cmdBuilder.Build();
await cmdParser.InvokeAsync(args);

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
                        services.PostConfigure<EventLogExtractConfiguration>(c => cmd.ParseOptions(c, invocation));
                    }
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
