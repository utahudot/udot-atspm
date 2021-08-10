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
                    //s.AddHostedService<PipelineBackgroundServiceTest>();

                    s.AddHostedService<ControllerLoggerHostService>();
                    s.AddTransient<ISignalControllerDownloader, ASCSignalControllerDownloader>();

                    //s.AddTransient<ISignalControllerDownloader, SignalControllerDownloaderStubA>();


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
                //.ConfigureLogging(c =>
                //{
                //    c.SetMinimumLevel(LogLevel.Debug);
                //})
                .UseConsoleLifetime()
                .Build();

            host.RunAsync();

            //SignalControllerDownloaderTest(host.Services);

            Console.ReadKey();
        }

        private async static void SignalControllerDownloaderTest(IServiceProvider serviceProvider)
        {
            List<Signal> signalList;
            
            using (var scope = serviceProvider.CreateScope())
            {
                //signalList = _serviceProvider.GetService<MOEContext>().Signals.Where(i => i.Enabled == true).Include(i => i.ControllerType).ToList();
                var db = scope.ServiceProvider.GetRequiredService<MOEContext>();
                signalList = db.Signals.Where(v => v.VersionActionId != 3).Include(i => i.ControllerType).AsNoTracking().AsEnumerable().GroupBy(r => r.SignalId).Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault()).ToList();
            }

            //ISignalControllerDownloader downloader = serviceProvider.GetService<ISignalControllerDownloader>();

            CancellationTokenSource cts = new CancellationTokenSource();

            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            List<Task> tasks = new List<Task>();
            foreach (Signal s in signalList)
            {
                var downloader = new ASCSignalControllerDownloader(serviceProvider.GetService<ILogger<ASCSignalControllerDownloader>>(), serviceProvider, serviceProvider.GetService<IOptions<SignalControllerDownloaderConfiguration>>());

                if (s.ControllerType.ControllerTypeId == Convert.ToInt32(downloader.ControllerType))
                {
                    //tasks.Add(Task.Run(async () =>
                    tasks.Add(new Task(async () =>
                    {
                        var downloader = new ASCSignalControllerDownloader(serviceProvider.GetService<ILogger<ASCSignalControllerDownloader>>(), serviceProvider, serviceProvider.GetService<IOptions<SignalControllerDownloaderConfiguration>>());

                        var result = await downloader.Execute(s, cts.Token);

                        //if (result == null)
                        //    Console.WriteLine($"Result: {s.SignalId} - {s.ControllerType.Description} - {result?.FullName}");

                        //Console.WriteLine($"Ellapsed Time: {sw.Elapsed}");
                    }));
                }
            }

            await tasks.StartAndWaitAllThrottledAsync(5, cts.Token);

            //await Task.WhenAll(tasks);

            //foreach (Signal s in signalList)
            //{
            //    var downloader = new ASCSignalControllerDownloader(serviceProvider.GetService<ILogger<ASCSignalControllerDownloader>>(), serviceProvider, serviceProvider.GetService<IOptions<SignalControllerDownloaderConfiguration>>());

            //    if (s.ControllerType.ControllerTypeId == Convert.ToInt32(downloader.ControllerType))
            //    {
            //        var result = await downloader.Execute(s, cts.Token);

            //        if (result == null)
            //            Console.WriteLine($"Result: {s.SignalId} - {s.ControllerType.Description} - {result?.FullName}");

            //        Console.WriteLine($"Ellapsed Time: {sw.Elapsed}");
            //    }
            //}

            //var pOptions = new ParallelOptions() {CancellationToken = cts.Token, MaxDegreeOfParallelism = 8 };

            //Parallel.ForEach(signalList, pOptions, async (s) =>
            //{
            //    var downloader = new ASCSignalControllerDownloader(serviceProvider.GetService<ILogger<ASCSignalControllerDownloader>>(), serviceProvider, serviceProvider.GetService<IOptions<SignalControllerDownloaderConfiguration>>());

            //    if (s.ControllerType.ControllerTypeId == Convert.ToInt32(downloader.ControllerType))
            //    {
            //        var result = await downloader.Execute(s, cts.Token);

            //        if (result == null)
            //            Console.WriteLine($"Result: {s.SignalId} - {s.ControllerType.Description} - {result?.FullName}");

            //        Console.WriteLine($"Ellapsed Time: {sw.Elapsed}");

            //    }
            //});

            Console.WriteLine($"Total Time: {sw.Elapsed}");

            sw.Stop();

            VerifyDirectories();
        }

        private static void VerifyDirectories()
        {
            DirectoryInfo dir = new DirectoryInfo("C:\\ControlLogs");

            var dirs = dir.GetDirectories();

            foreach (var d in dirs)
            {
                var f = d.GetFiles("*.*", SearchOption.AllDirectories).ToList();

                if (f.Count == 0)
                {
                    Console.WriteLine($"Empty: {d.FullName}");
                }
                else
                {
                    Console.WriteLine($"STUFF: {d.FullName} - {f.Count}");
                }
            }
        }
    }
}