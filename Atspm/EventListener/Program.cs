using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.Atspm.Infrastructure.Services.Listeners;
using Utah.Udot.Atspm.Infrastructure.Services.Receivers; // UDPSpeedBatchListener

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((h, l) =>
    {
        if (OperatingSystem.IsWindows())
        {
            l.AddEventLog(c =>
            {
                c.SourceName = AppDomain.CurrentDomain.FriendlyName;
                c.LogName = "Atspm";
            });
        }

        //l.AddGoogle(new LoggingServiceOptions
        //{
        //    ServiceName = AppDomain.CurrentDomain.FriendlyName,
        //    Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
        //    Options = LoggingOptions.Create(LogLevel.Information, AppDomain.CurrentDomain.FriendlyName)
        //});
    })
    .ConfigureServices((context, services) =>
    {
        // 1) Bind batch options from config
        services.Configure<EventListenerConfiguration>(context.Configuration.GetSection("EventListenerConfiguration"));

        // 2) Register an HttpClient named "IngestApi" pointing at your DataApi
        services.AddHttpClient("IngestApi", (sp, client) =>
          {
              var cfg = sp.GetRequiredService<IOptions<EventListenerConfiguration>>().Value;
              client.BaseAddress = new Uri(cfg.ApiBaseUrl);
          })

        // add this handler to skip SSL validation for testing
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        services.AddAtspmDbContext(context);
        services.AddAtspmEFConfigRepositories();

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


        services.AddSingleton<UDPSpeedBatchListener>();
        services.AddSingleton<TCPSpeedBatchListener>();


        // 4) Host it as a background service
        services.AddHostedService<EventListenerWorker>();
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
