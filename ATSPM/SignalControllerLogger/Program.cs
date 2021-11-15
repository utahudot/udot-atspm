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
using ATSPM.Application.Enums;
using System.Xml;
using System.Xml.Linq;

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
                    s.AddDbContext<DbContext,MOEContext>(); //b => b.UseLazyLoadingProxies().UseChangeTrackingProxies()

                    s.AddHostedService<ControllerLoggerBackgroundService>();

                    s.AddTransient<ISignalControllerDownloader, ASCSignalControllerDownloader>();
                    s.AddTransient<ISignalControllerDownloader, MaxTimeSignalControllerDownloader>();

                    //s.AddTransient<ISignalControllerDecoder, ASCSignalControllerDecoder>();
                    s.AddTransient<ISignalControllerDecoder, MaxTimeSignalControllerDecoder>();

                    s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();


                    //s.AddDbContext<MOEContext>();
                    //s.AddHostedService<FileETLHostedService>();
                    //s.AddHostedService<ControllerFTPServiceTest>();
                    //s.AddHostedService<ControllerFTPService>();
                    //s.AddSingleton<MOEContext>();
                    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0
                    s.Configure<FileETLSettings>(h.Configuration.GetSection("FileETLSettings"));
                    //s.Configure<ControllerFTPSettings>(h.Configuration.GetSection("ControllerFTPSettings"));
                    s.Configure<SignalControllerDownloaderConfiguration>(h.Configuration.GetSection("SignalControllerDownloaderConfiguration"));
                })
                .ConfigureLogging(c =>
                {
                    c.SetMinimumLevel(LogLevel.Information);
                })
                .UseConsoleLifetime()
                .Build();

            //host.RunAsync();


            Signal s1 = new Signal()
            {
                Ipaddress = "10.209.2.120",
                //Ipaddress = "hello",
                Enabled = true,
                PrimaryName = "Maxtime Test",
                SignalId = "0",
                ControllerTypeId = 4,
                ControllerType = new ControllerType() { ControllerTypeId = 4 }
            };

            Signal s2 = new Signal()
            {
                Ipaddress = "10.209.2.108",
                Enabled = true,
                PrimaryName = "Cobalt Test",
                SignalId = "9731",
                ControllerTypeId = 2,
                ControllerType = new ControllerType() { ControllerTypeId = 2 }
            };

            var l = new List<Signal>();
            l.Add(s1);
            //l.Add(s2);

            //var d = host.Services.GetService<ISignalControllerDownloader>();

            //Console.WriteLine($"CanExecute: {d.CanExecute(s1)}");

            //Task.Run(() => d.ExecuteAsync(s1));

            //var downloaders = host.Services.GetServices<ISignalControllerDownloader>();

            //Console.WriteLine($"Downloader Count: {downloaders.Count()}");

            //foreach (Signal s in l)
            //{
            //    var downloader = downloaders.FirstOrDefault(d => d.CanExecute(s));

            //    if (downloader != null)
            //    {
            //        Console.WriteLine($"Downloader: {downloader.CanExecute(s)} - {s.PrimaryName} - {downloader.GetType().Name}");

            //        Task.Run(() => downloader.ExecuteAsync(s)).ContinueWith(t => Console.WriteLine($"task result: {t.Exception}-{t.Status}"));


            //    }
            //}


            var decoder = host.Services.GetService<ISignalControllerDecoder>();
            var file1 = new FileInfo("C:\\ControlLogs\\0\\0_637709235474596368.xml");
            var file2 = new FileInfo("C:\\ControlLogs\\9731\\ECON_10.204.12.167_2021_08_09_1831.dat");
            var file3 = new FileInfo("C:\\ControlLogs\\9731\\ECON_10.204.7.239_2021_08_09_1841.datZ");

            Console.WriteLine($"IsCompressed: {decoder.IsCompressed(file1.ToMemoryStream())}");
            Console.WriteLine($"IsEncoded: {decoder.IsEncoded(file1.ToMemoryStream())}");
            Console.WriteLine($"CanExecute: {decoder.CanExecute(file1)}");

            Console.WriteLine($"IsCompressed: {decoder.IsCompressed(file2.ToMemoryStream())}");
            Console.WriteLine($"IsEncoded: {decoder.IsEncoded(file2.ToMemoryStream())}");
            Console.WriteLine($"CanExecute: {decoder.CanExecute(file2)}");

            Console.WriteLine($"IsCompressed: {decoder.IsCompressed(file3.ToMemoryStream())}");
            Console.WriteLine($"IsEncoded: {decoder.IsEncoded(file3.ToMemoryStream())}");
            Console.WriteLine($"CanExecute: {decoder.CanExecute(file2)}");

            var logs = decoder.ExecuteAsync(file1);

            logs.ContinueWith(t => 
            { 
                Console.WriteLine($"ExecuteAsync: {t.Result.Count}");
                
                foreach (ControllerEventLog log in t.Result)
                {
                    Console.WriteLine($"{log.SignalId} - {log.EventCode} - {log.EventParam} - {log.Timestamp}");
                }
            });



            //using (FileStream stream = new FileStream("C:\\ControlLogs\\0\\0_637709235474596368.xml", FileMode.Open))
            //{
            //    //StepList result = (StepList)serializer.Deserialize(fileStream);
            //    XDocument xml = XDocument.Load(stream);
            //    //var test = xml.Descendants("EventResponses");
            //    foreach (var a in xml.Descendants().Where(d => d.Name == "Event"))
            //    {
            //        Console.WriteLine($"stuff: {a.Attribute("Parameter").Value} - {a.Attribute("EventTypeID").Value} - {a.Attribute("TimeStamp").Value}  - {a.Attribute("ID").Value}");
            //    }
            //}


            Console.ReadKey();
        }
    }

    
}