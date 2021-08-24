using ATSPM.Application.Configuration;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Infrasturcture.Data;
using ATSPM.Infrasturcture.Services.ControllerDownloaders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATSPM.Domain.Extensions;
using ATSPM.Infrasturcture.Services.ControllerDecoders;
using ATSPM.Infrasturcture.Repositories;
using ATSPM.Application.Repositories;

namespace ATSPM.SignalControllerLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            //register based on environment https://stackoverflow.com/questions/59501699/dependency-injection-call-different-services-based-on-the-environment

            var host = new HostBuilder()
                .ConfigureLogging((h, l) =>
                {
                    l.AddConsole();
                })
                .ConfigureHostConfiguration(b =>
                {
                    b.SetBasePath(Directory.GetCurrentDirectory());
                })
                .ConfigureAppConfiguration(b =>
                {
                    b.SetBasePath(Directory.GetCurrentDirectory());
                    b.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((h, s) =>
                {
                    s.AddLogging();
                    s.AddDbContext<MOEContext>(); //b => b.UseLazyLoadingProxies().UseChangeTrackingProxies()

                    s.AddHostedService<ControllerLoggerBackgroundService>();
                    s.AddTransient<ISignalControllerDownloader, ASCSignalControllerDownloader>();
                    s.AddTransient<ISignalControllerDecoder, ASCSignalControllerDecoder>();

                    s.AddTransient<IControllerEventLogRepository, ControllerEventLogEFRepository>();


                    //s.AddDbContext<MOEContext>();
                    //s.AddHostedService<FileETLHostedService>();
                    //s.AddHostedService<ControllerFTPServiceTest>();
                    //s.AddHostedService<ControllerFTPService>();
                    //s.AddSingleton<MOEContext>();
                    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0
                    //s.Configure<FileETLSettings>(h.Configuration.GetSection("FileETLSettings"));
                    //s.Configure<ControllerFTPSettings>(h.Configuration.GetSection("ControllerFTPSettings"));
                    s.Configure<SignalControllerDownloaderConfiguration>(h.Configuration.GetSection("SignalControllerDownloaderConfiguration"));
                })
                .ConfigureLogging(c =>
                {
                    c.SetMinimumLevel(LogLevel.Debug);
                })
                .UseConsoleLifetime()
                .Build();

            //host.RunAsync();

            var repo = host.Services.GetService<IControllerEventLogRepository>();



            Console.ReadKey();
        }
    }
}