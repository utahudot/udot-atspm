using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.EventLogUtility;
using ATSPM.EventLogUtility.CommandBinders;
using ATSPM.EventLogUtility.Commands;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;


//var handler = CommandHandler.Create((InvocationContext invocation) =>
//{
//    Console.WriteLine($"Command: {invocation.ParseResult.CommandResult.Command.Name} - {invocation.ParseResult.CommandResult.Command.Description}");

//    foreach (var c in invocation.ParseResult.CommandResult.Children)
//    {
//        var s = c.Symbol;
//        Console.WriteLine($"symbol: {s.Name} - {s.Description} - {c.Tokens}");   
//    }
//});

var rootCmd = new EventLogCommands();

var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();



//cmdBuilder.AddMiddleware(async (i, n) =>
//{
//    i.BindingContext.AddService(typeof(IInject), s => new Inject());
//    await n(i);
//});


//cmdBuilder.UseHost(a => Host.CreateDefaultBuilder(), h =>
//{
//    h.ConfigureServices((h, s) =>
//    {
//        //databases
//        s.AddDbContext<EventLogContext>(db => db.UseSqlServer(h.Configuration.GetConnectionString(nameof(EventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(h.HostingEnvironment.IsDevelopment()));

//        //repositories
//        s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();

//        s.AddSingleton(binder);
//        s.AddOptions<EventLogExtractConfiguration>().BindCommandLine();

        

//        //Hosted services
//        //s.AddHostedService<ExportUtilityService>();
//    })
//    .UseConsoleLifetime();

//});

var cmdParser = cmdBuilder.Build();
await cmdParser.InvokeAsync("log -d 10/10/2020 -i 1001 1002");

//var cmdParser = cmdBuilder.Build();
//await cmdParser.InvokeAsync(args);




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

                    //if (invocation.ParseResult.CommandResult.Command is ExtractConsoleCommand cmd)
                    //{
                    //    services.PostConfigure<EventLogExtractConfiguration>(c => cmd.ParseOptions(c, invocation));
                    //}
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
