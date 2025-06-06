using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.Atspm.Infrastructure.Services.Listeners;
using Utah.Udot.Atspm.Infrastructure.Services.Receivers; // UDPSpeedBatchListener


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var isWindows = OperatingSystem.IsWindows();

var host = Host.CreateDefaultBuilder(args)
    .UseContentRoot(AppContext.BaseDirectory) // works for services and Docker
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables(); // supports Docker env vars
    })
    .UseWindowsService() // harmless on Linux
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        logging.AddConsole();

        if (isWindows)
        {
            logging.AddEventLog(options =>
            {
                options.SourceName = "EventListenerService";
                options.LogName = "Atspm";
            });
        }
    })
    .ConfigureServices((context, services) =>
    {
        // Your normal registrations
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
        services.AddSingleton<ITcpReceiver>(sp =>
            new TcpReceiver(
                sp.GetRequiredService<IOptions<EventListenerConfiguration>>().Value.TcpPort,
                sp.GetRequiredService<ILogger<TcpReceiver>>()
            ));

        services.AddScoped<UDPSpeedBatchListener>();
        services.AddScoped<TCPSpeedBatchListener>();
        services.AddHostedService<EventListenerWorker>();

    })
    .UseConsoleLifetime()
    .Build();

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine("Unhandled startup error:");
    Console.WriteLine(ex.ToString());
    throw; // re-throw so debugger catches it too
}


