using ATSPM.Application.Configuration;
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
                    s.AddTransient<ISignalControllerDownloader, FTPSignalControllerDownloader>();
                    s.AddTransient<ISignalControllerDownloader, MaxTimeSignalControllerDownloader>();


                    //decoders
                    s.AddTransient<ISignalControllerDecoder, ASCSignalControllerDecoder>();
                    s.AddTransient<ISignalControllerDecoder, MaxTimeSignalControllerDecoder>();

                    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0
                    //s.Configure<FileETLSettings>(h.Configuration.GetSection("FileETLSettings"));
                    s.Configure<ControllerFTPSettings>(h.Configuration.GetSection("ControllerFTPSettings"));
                    s.Configure<SignalControllerDownloaderConfiguration>(h.Configuration.GetSection("SignalControllerDownloaderConfiguration"));
                    s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));
                })

                .UseConsoleLifetime()
                .Build();

            //host.RunAsync();
            //1St0p$h0p
            //ecpi2ecpi


            


            

            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: Start");

            

            var block = new TransformManyBlock<int, FileInfo>(async s =>
            {
                var list = new List<FileInfo>();

                await foreach (var item in AsyncTest())
                {
                    list.Add(item);
                }

                return list;



            });

            block.Post(1);

            //await foreach (var item in AsyncTest())
            await foreach (var item in block.ReceiveAllAsync())
            {
                Console.WriteLine($"{DateTime.Now}: {item}");
            }

            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: End");

            Console.ReadKey();
        }

        private static async IAsyncEnumerable<FileInfo> AsyncTest()
        {
            //var dirFiles = new List<FileInfo>();
            
            var connectionInfo = new ConnectionInfo("10.209.2.126", "econolite", new PasswordAuthenticationMethod("econolite", "ecpi2ecpi"));

            using (var client = new SftpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                }
                catch (SshException e)
                {
                    Console.WriteLine($"SshException: {e}");
                }
                catch (IOException e)
                {
                    Console.WriteLine($"IOException: {e}");
                }

                if (client.IsConnected)
                {
                    var files = client.ListDirectory("/opt/econolite/set1").Where(f => f.FullName.Contains(".dat")).ToList();

                    foreach (var file in files)
                    {
                        Console.WriteLine($"File: {file.FullName}");

                        using (FileStream fileStream = File.OpenWrite(Path.Combine(@"C:\ControlLogs", file.Name)))
                        {
                            try
                            {
                                await Task.Factory.FromAsync(client.BeginDownloadFile(file.FullName, fileStream), client.EndDownloadFile);
                            }
                            catch (SshException e)
                            {
                                Console.WriteLine($"SshException: {e}");
                            }
                            catch (IOException e)
                            {
                                Console.WriteLine($"IOException: {e}");
                            }

                            var fileInfo = new FileInfo(fileStream.Name);

                            if (fileInfo.Exists)
                                yield return fileInfo;
                        }
                    }

                    client.Disconnect();
                }
            }

            //return dirFiles;
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