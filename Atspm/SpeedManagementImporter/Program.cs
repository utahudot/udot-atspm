using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpeedManagementImporter;
using SpeedManagementImporter.Services.Atspm;
using SpeedManagementImporter.Services.Clearguide;
using SpeedManagementImporter.Services.Pems;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand();

        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();

        // Configure logging services explicitly
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();  // Clear any default providers
            loggingBuilder.AddConsole();      // Add console logging
            loggingBuilder.AddDebug();        // Add debug logging (optional)
            loggingBuilder.SetMinimumLevel(LogLevel.Trace); // Set minimum log level to Trace
        });

        // Register configuration as IConfiguration
        services.AddSingleton<IConfiguration>(configuration);

        // Register download services
        services.AddScoped<PemsDownloaderService>();
        services.AddScoped<AtspmDownloaderService>();
        services.AddScoped<ClearguideFileDownloaderService>();
        services.AddScoped<ClearguideFileUploader>();

        // Set Google Cloud Credentials
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", configuration["GoogleApplicationCredentials"]);

        ServiceRegistrations.AddRepositories(services, configuration);

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application Starting");

        // Resolve the dependencies
        var pemsDownloader = serviceProvider.GetRequiredService<PemsDownloaderService>();
        var atspmDownloader = serviceProvider.GetRequiredService<AtspmDownloaderService>();
        var clearguideDownloader = serviceProvider.GetRequiredService<ClearguideFileDownloaderService>();
        var clearguideUploader = serviceProvider.GetRequiredService<ClearguideFileUploader>();

        // Add the download command
        rootCommand.AddCommand(new DownloadCommand(pemsDownloader, atspmDownloader, clearguideDownloader, clearguideUploader));

        return await rootCommand.InvokeAsync(args);
    }
}
