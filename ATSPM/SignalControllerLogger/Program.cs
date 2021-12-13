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


            var test = host.Services.GetService<IControllerEventLogRepository>();
            var db = host.Services.GetService<DbContext>();

            var log = new ControllerLogArchive() { SignalId = "1234", ArchiveDate = DateTime.Now };

            var list = new List<ControllerEventLog>();

            list.Add(new ControllerEventLog() { EventCode = 1, EventParam = 101, SignalId = "1234", Timestamp = DateTime.Now });
            list.Add(new ControllerEventLog() { EventCode = 2, EventParam = 102, SignalId = "1234", Timestamp = DateTime.Now });
            list.Add(new ControllerEventLog() { EventCode = 3, EventParam = 103, SignalId = "1234", Timestamp = DateTime.Now });

            log.LogData = list;

            test.Add(log);



            var specification = new ControllerLogDateRangeSpecification("1234", DateTime.Now.Subtract(TimeSpan.FromHours(24)), DateTime.Now.Subtract(TimeSpan.FromHours(0)));



            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Path.Combine("C:", "ControlLogs", $"{log.ArchiveDate.Year}", $"{log.ArchiveDate.Month}", $"{log.ArchiveDate.Day}"));

            if (!dir.Exists)
                dir.Create();

            var fileQuery = dir.GetFiles("ControllerLogArchive*.json", System.IO.SearchOption.AllDirectories).AsQueryable();





            var result1 = fileQuery.GetModelKeys<ControllerLogArchive>(db);

            foreach (ControllerLogArchive item in result1)
            {
                Console.WriteLine($"result1: {item}");
            }

            var result2 = result1.FromSpecification(specification);

            foreach (ControllerLogArchive item in result2)
            {
                Console.WriteLine($"result2: {item}");
            }

            Console.ReadKey();
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