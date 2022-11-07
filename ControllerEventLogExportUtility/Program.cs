using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using ControllerEventLogExportUtility;
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
    //var h = Host.CreateDefaultBuilder()
    h.ConfigureAppConfiguration((h, c) =>
    {
        //c.AddUserSecrets("af468330-96e6-4297-a188-86f216ee07b4");
        //c.AddCommandLine(args);
        //c.AddCommandLineDirectives(result, "name");
    })
    .ConfigureServices((h, s) =>
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

var parser = cmdBuilder.Build();
await parser.InvokeAsync(args);




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
                await next(invocation);
                await host.StopAsync();
            });
}
