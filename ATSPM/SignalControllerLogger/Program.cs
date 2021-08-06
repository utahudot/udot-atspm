using ControllerLogger.Data;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.Extensions.Hosting;
using ControllerLogger.Services;
using ControllerLogger.Configuration;
using System.Net;
using ControllerLogger.Helpers;
using FluentFTP;
using ControllerLogger.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using ControllerLogger.ServiceHosts;
using ControllerLogger.Application.Services;
using ControllerLogger.Application.Configuration;

namespace ControllerLogger
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
                    //s.AddHostedService<PipelineBackgroundServiceTest>();

                    s.AddHostedService<ControllerLoggerHostService>();
                    s.AddTransient<ISignalControllerDownloader, ASCSignalControllerDownloader>();

                    s.AddTransient<ISignalControllerDownloader, SignalControllerDownloaderTestA>();


                    //s.AddDbContext<MOEContext>();
                    //s.AddHostedService<FileETLHostedService>();
                    //s.AddHostedService<ControllerFTPServiceTest>();
                    //s.AddHostedService<ControllerFTPService>();
                    //s.AddSingleton<MOEContext>();
                    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0
                    s.Configure<FileETLSettings>(h.Configuration.GetSection("FileETLSettings"));
                    s.Configure<ControllerFTPSettings>(h.Configuration.GetSection("ControllerFTPSettings"));
                    s.Configure<SignalControllerDownloaderConfiguration>(h.Configuration.GetSection("SignalControllerDownloaderConfiguration"));
                })
                //.ConfigureLogging(c =>
                //{
                //    c.SetMinimumLevel(LogLevel.Debug);
                //})
                .UseConsoleLifetime()
                .Build();

            host.RunAsync();

            Console.ReadKey();
        }
    }
}