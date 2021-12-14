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
using System.Text.Json;
using System.Text;
using ATSPM.Application.Specifications;
using System.Linq.Expressions;
using ATSPM.Domain.Specifications;

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

                    //repositories
                    s.AddScoped<ISignalRepository, SignalEFRepository>();
                    //s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();


                    s.AddScoped<IControllerEventLogRepository, ControllerEventLogParquetRepository>();

                    //background services
                    s.AddHostedService<ControllerLoggerBackgroundService>();

                    //downloaders
                    s.AddTransient<ISignalControllerDownloader, ASCSignalControllerDownloader>();
                    s.AddTransient<ISignalControllerDownloader, MaxTimeSignalControllerDownloader>();


                    //decoders
                    //s.AddTransient<ISignalControllerDecoder, ASCSignalControllerDecoder>();
                    s.AddTransient<ISignalControllerDecoder, MaxTimeSignalControllerDecoder>();




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


            var repo = host.Services.GetService<IControllerEventLogRepository>();

            var start = DateTime.Now.Subtract(TimeSpan.FromDays(5));
            var end = DateTime.Now.Subtract(TimeSpan.FromDays(1));

            var events = repo.GetEventsBetweenDates(start, end);

            foreach (var e in events)
            {
                Console.WriteLine($"event: {e}");
            }

            Console.ReadKey();
        }

        public static IList<ControllerLogArchive> GenerateLogArchives()
        {
            List<ControllerLogArchive> archives = new List<ControllerLogArchive>();
            var random = new Random();

            foreach (int i in Enumerable.Range(1, 100))
            {
                var archive = new ControllerLogArchive() { SignalId = $"{1000 + i}", ArchiveDate = DateTime.Now.Subtract(TimeSpan.FromDays(random.Next(1, 10))) };
                var list = new List<ControllerEventLog>();

                list.Add(new ControllerEventLog() { EventCode = random.Next(1,50), EventParam = random.Next(1, 50) + 100, SignalId = archive.SignalId, Timestamp = archive.ArchiveDate });
                list.Add(new ControllerEventLog() { EventCode = random.Next(1, 50), EventParam = random.Next(1, 50) + 100, SignalId = archive.SignalId, Timestamp = archive.ArchiveDate });
                list.Add(new ControllerEventLog() { EventCode = random.Next(1, 50), EventParam = random.Next(1, 50) + 100, SignalId = archive.SignalId, Timestamp = archive.ArchiveDate });

                archive.LogData = list;

                archives.Add(archive);
            }

            return archives;
            
        }
    }

    public static class FileInfoExtensions
    {
        public static IQueryable<T> GetModelKeys<T>(this IQueryable<FileInfo> files, DbContext db) where T : ATSPMModelBase
        {
            return files.Select(s => s.ToModelKeys<T>(db));
        }

        public static T ToModelKeys<T>(this FileInfo file, DbContext db) where T : ATSPMModelBase
        {
            var split = file.Name.Substring(0, file.Name.IndexOf(".")).Split("_").ToList();

            if (split[0] == typeof(T).Name)
            {
                split.RemoveAt(0);

                var model = Activator.CreateInstance(typeof(T));

                var keys = db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.Name).Zip(split).ToDictionary(d => d.First, d => d.Second);

                foreach (var t in model.GetType().GetProperties().Where(p => keys.Keys.Contains(p.Name)))
                {
                    if (t.PropertyType == typeof(DateTime))
                    {
                        t.SetValue(model, DateTime.ParseExact(keys[t.Name], "dd-MM-yyyy", null));
                    }
                    else
                    {
                        t.SetValue(model, Convert.ChangeType(keys[t.Name], t.PropertyType));
                    }
                }

                return (T)model;
            }

            return null;
        }
    }
}