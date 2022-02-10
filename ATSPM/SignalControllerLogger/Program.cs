using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.Common;
using ATSPM.Infrasturcture.Converters;
using ATSPM.Infrasturcture.Data;
using ATSPM.Infrasturcture.Repositories;
using ATSPM.Infrasturcture.Services.ControllerDecoders;
using ATSPM.Infrasturcture.Services.ControllerDownloaders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.SignalControllerLogger
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //register based on environment https://stackoverflow.com/questions/59501699/dependency-injection-call-different-services-based-on-the-environment



            var host = Host.CreateDefaultBuilder()
            //var host = new HostBuilder()

                .ConfigureLogging((h, l) =>
                {
                    l.SetMinimumLevel(LogLevel.None);
                    l.AddConsole();
                })
                //.ConfigureHostConfiguration(b =>
                //{
                //    b.SetBasePath(Directory.GetCurrentDirectory());
                //})
                //.ConfigureAppConfiguration(b =>
                //{
                //    b.SetBasePath(Directory.GetCurrentDirectory());
                //    b.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                //    b.AddEnvironmentVariables();
                //})
                .ConfigureServices((h, s) =>
                {
                    s.AddLogging();
                    s.AddDbContext<DbContext, MOEContext>(db => db.UseSqlServer(h.Configuration.GetConnectionString(h.HostingEnvironment.EnvironmentName))); //b => b.UseLazyLoadingProxies().UseChangeTrackingProxies()


                    //repositories
                    s.AddScoped<ISignalRepository, SignalEFRepository>();
                    //s.AddScoped<ISignalRepository, SignalFileRepository>();
                    s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
                    //s.AddScoped<IControllerEventLogRepository, ControllerEventLogFileRepository>();


                    //s.AddTransient<IFileTranscoder, JsonFileTranscoder>();
                    //s.AddTransient<IFileTranscoder, ParquetFileTranscoder>();
                    s.AddTransient<IFileTranscoder, CompressedJsonFileTranscoder>();


                    

                    //background services
                    //s.AddHostedService<ControllerLoggerBackgroundService>();
                    s.AddHostedService<TPLDataflowService>();

                    //downloaders
                    //s.AddTransient<ISignalControllerDownloader, FTPSignalControllerDownloader>();
                    //s.AddTransient<ISignalControllerDownloader, MaxTimeSignalControllerDownloader>();
                    s.AddTransient<ISignalControllerDownloader, SFTPSignalControllerDownloader>();
                    //s.AddTransient<ISignalControllerDownloader, StubSignalControllerDownloader>();


                    //decoders
                    s.AddTransient<ISignalControllerDecoder, ASCSignalControllerDecoder>();
                    s.AddTransient<ISignalControllerDecoder, MaxTimeSignalControllerDecoder>();

                    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0

                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(SFTPSignalControllerDownloader), h.Configuration.GetSection(nameof(SFTPSignalControllerDownloader)));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(FTPSignalControllerDownloader), h.Configuration.GetSection(nameof(FTPSignalControllerDownloader)));



                    s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));
                })

                .UseConsoleLifetime()
                .Build();

            //host.RunAsync();
            //1St0p$h0p
            //ecpi2ecpi

            //using (var scope = host.Services.CreateScope())
            //{
            //    var temp = scope.ServiceProvider.GetServices<IOptionsMonitor<SignalControllerDownloaderConfiguration>>();
            //}

            var temp = host.Services.GetService<IOptionsMonitor<SignalControllerDownloaderConfiguration>>();

            var temp1 = temp.Get("SFTPSignalControllerDownloader");



            temp.OnChange((c, s) => Console.WriteLine($"options have changed: {c.ConnectionTimeout} - {s}"));

            //IReadOnlyList<Signal> _signalList;
            //using (var scope = host.Services.CreateScope())
            //{
            //    _signalList = scope.ServiceProvider.GetService<ISignalRepository>().GetLatestVersionOfAllSignals().Take(1).ToList();
            //}

            //var testSignal = new Signal()
            //{
            //    SignalId = "9704",
            //    Ipaddress = "10.209.2.126",
            //    ControllerType = new ControllerType()
            //    {
            //        ControllerTypeId = 2,
            //        ActiveFtp = true,
            //        Ftpdirectory = "/set1",
            //        UserName = "econolite",
            //        Password = "ecpi2ecpi"
            //    }
            //};

            //_signalList = new List<Signal>(new Signal[] { testSignal });

            //var progress = new Progress<ControllerDownloadProgress>(p => Console.WriteLine($"Progress={p}"));

            //ISignalControllerDownloader downloader = host.Services.GetService<ISignalControllerDownloader>();

            //foreach (var signal in _signalList)
            //{
            //    //var items = downloader.Execute(signal, progress);

            //    await foreach (var item in downloader.Execute(signal, progress))
            //    {
            //        Console.WriteLine($"Returned: {item.FullName}");
            //    }
            //}


            Console.ReadKey();
        }

        

    //    public static IList<ControllerLogArchive> GenerateLogArchives()
    //    {
    //        List<ControllerLogArchive> archives = new List<ControllerLogArchive>();
    //        var random = new Random();

    //        foreach (int i in Enumerable.Range(1, 100))
    //        {
    //            var archive = new ControllerLogArchive() { SignalId = $"{1000 + i}", ArchiveDate = DateTime.Now.Subtract(TimeSpan.FromDays(random.Next(1, 10))) };
    //            var list = new List<ControllerEventLog>();

    //            list.Add(new ControllerEventLog() { EventCode = random.Next(1,50), EventParam = random.Next(1, 50) + 100, SignalId = archive.SignalId, Timestamp = archive.ArchiveDate });
    //            list.Add(new ControllerEventLog() { EventCode = random.Next(1, 50), EventParam = random.Next(1, 50) + 100, SignalId = archive.SignalId, Timestamp = archive.ArchiveDate });
    //            list.Add(new ControllerEventLog() { EventCode = random.Next(1, 50), EventParam = random.Next(1, 50) + 100, SignalId = archive.SignalId, Timestamp = archive.ArchiveDate });

    //            archive.LogData = list;

    //            archives.Add(archive);
    //        }

    //        return archives;
            
    //    }
    //}

    //public static class FileInfoExtensions
    //{
    //    public static IQueryable<T> GetModelKeys<T>(this IQueryable<FileInfo> files, DbContext db) where T : ATSPMModelBase
    //    {
    //        return files.Select(s => s.ToModelKeys<T>(db));
    //    }

    //    public static T ToModelKeys<T>(this FileInfo file, DbContext db) where T : ATSPMModelBase
    //    {
    //        var split = file.Name.Substring(0, file.Name.IndexOf(".")).Split("_").ToList();

    //        if (split[0] == typeof(T).Name)
    //        {
    //            split.RemoveAt(0);

    //            var model = Activator.CreateInstance(typeof(T));

    //            var keys = db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.Name).Zip(split).ToDictionary(d => d.First, d => d.Second);

    //            foreach (var t in model.GetType().GetProperties().Where(p => keys.Keys.Contains(p.Name)))
    //            {
    //                if (t.PropertyType == typeof(DateTime))
    //                {
    //                    t.SetValue(model, DateTime.ParseExact(keys[t.Name], "dd-MM-yyyy", null));
    //                }
    //                else
    //                {
    //                    t.SetValue(model, Convert.ChangeType(keys[t.Name], t.PropertyType));
    //                }
    //            }

    //            return (T)model;
    //        }

    //        return null;
    //    }
    }
}