using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Infrastructure.Services.Listeners;
using Utah.Udot.Atspm.Infrastructure.Services.Receivers; // UDPSpeedBatchListener

var host = Host.CreateDefaultBuilder(args)
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

        services.AddSingleton<IUdpReceiver>(sp =>
            new UdpReceiver(sp.GetRequiredService<IOptions<EventListenerConfiguration>>().Value.UdpPort));
        services.AddSingleton<ITcpReceiver>(sp =>
            new TcpReceiver(sp.GetRequiredService<IOptions<EventListenerConfiguration>>().Value.TcpPort));

        services.AddSingleton<UDPSpeedBatchListener>();
        services.AddSingleton<TCPSpeedBatchListener>();


        // 4) Host it as a background service
        services.AddHostedService<EventListenerWorker>();
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
