using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.Atspm.Infrastructure.Services.Listeners;
using Utah.Udot.Atspm.Infrastructure.Services.Receivers;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure Serilog *before* host creation
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "eventlistener.log"))
    .CreateLogger();

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog() // replaces default logging with Serilog
        .UseContentRoot(AppContext.BaseDirectory)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddEnvironmentVariables();
        })
        .UseWindowsService()
        .ConfigureServices((context, services) =>
        {
            services.Configure<EventListenerConfiguration>(context.Configuration.GetSection("EventListenerConfiguration"));

            services.AddHttpClient("IngestApi", (sp, client) =>
            {
                var cfg = sp.GetRequiredService<IOptions<EventListenerConfiguration>>().Value;
                client.BaseAddress = new Uri(cfg.ApiBaseUrl);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

            services.AddAtspmDbContext(context);
            services.AddAtspmEFConfigRepositories();
            services.AddAtspmEFEventLogRepositories();
            services.AddEventPublishers(context);

            services.AddSingleton<IUdpReceiver>(sp =>
                new UdpReceiver(
                    sp.GetRequiredService<IOptions<EventListenerConfiguration>>().Value.UdpPort,
                    sp.GetRequiredService<ILogger<UdpReceiver>>()
                ));

            services.AddScoped<UDPSpeedBatchListener>();
            services.AddHostedService<EventListenerWorker>();
        })
        //.UseConsoleLifetime()
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled startup error");
}
finally
{
    Log.CloseAndFlush();
}
