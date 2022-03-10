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
using FluentFTP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
                    l.SetMinimumLevel(LogLevel.Warning);
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

                    //background services
                    s.AddHostedService<TPLDataflowService>();

                    //repositories
                    s.AddScoped<ISignalRepository, SignalEFRepository>();
                    //s.AddScoped<ISignalRepository, SignalFileRepository>();
                    s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
                    //s.AddScoped<IControllerEventLogRepository, ControllerEventLogFileRepository>();


                    //s.AddTransient<IFileTranscoder, JsonFileTranscoder>();
                    //s.AddTransient<IFileTranscoder, ParquetFileTranscoder>();
                    s.AddTransient<IFileTranscoder, CompressedJsonFileTranscoder>();

                    //downloader clients
                    s.AddTransient<IHTTPDownloaderClient, HttpDownloaderClient>();
                    s.AddTransient<IFTPDownloaderClient, FluentFTPDownloaderClient>();
                    s.AddTransient<ISFTPDownloaderClient, SSHNetSFTPDownloaderClient>();

                    //downloaders
                    s.AddScoped<ISignalControllerDownloader, ASC3SignalControllerDownloader>();
                    s.AddScoped<ISignalControllerDownloader, CobaltSignalControllerDownloader>();
                    s.AddScoped<ISignalControllerDownloader, MaxTimeSignalControllerDownloader>();
                    s.AddScoped<ISignalControllerDownloader, EOSSignalControllerDownloader>();
                    s.AddScoped<ISignalControllerDownloader, NewCobaltSignalControllerDownloader>();

                    //decoders
                    s.AddTransient<ISignalControllerDecoder, ASCSignalControllerDecoder>();
                    s.AddTransient<ISignalControllerDecoder, MaxTimeSignalControllerDecoder>();

                    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0
                    //downloader configurations
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(ASC3SignalControllerDownloader), h.Configuration.GetSection(nameof(ASC3SignalControllerDownloader)));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(CobaltSignalControllerDownloader), h.Configuration.GetSection(nameof(CobaltSignalControllerDownloader)));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(MaxTimeSignalControllerDownloader), h.Configuration.GetSection(nameof(MaxTimeSignalControllerDownloader)));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(EOSSignalControllerDownloader), h.Configuration.GetSection(nameof(EOSSignalControllerDownloader)));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(NewCobaltSignalControllerDownloader), h.Configuration.GetSection(nameof(NewCobaltSignalControllerDownloader)));

                    s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));
                })

                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();

            //var _signalList = new List<Signal>();

            //using (var scope = host.Services.CreateScope())
            //{
            //    _signalList = scope.ServiceProvider.GetService<ISignalRepository>().GetLatestVersionOfAllSignals().Where(w => w.ControllerType.ControllerTypeId == 9).Take(25).ToList();
            //}

            //foreach (var s in _signalList)
            //{
            //    IFtpClient Client = null;
            //    FtpListItem[] results = null;

            //    try
            //    {
            //        var credentials = new NetworkCredential(s.ControllerType?.UserName, s.ControllerType?.Password, s.Ipaddress);

            //        Client ??= new FtpClient(credentials.Domain, credentials);

            //        Client.ConnectTimeout = 2000;
            //        Client.ReadTimeout = 2000;
            //        Client.DataConnectionType = FtpDataConnectionType.AutoActive;

            //        //await Client.AutoConnectAsync(token);
            //        await Client.ConnectAsync();
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine($"Connection Error: {e}");
            //    }

            //    if (Client.IsConnected)
            //    {
            //        try
            //        {
            //            //"/econolite/set1"
            //            results = await Client.GetListingAsync("/opt/econolite/set1", FtpListOption.Auto);
            //        }
            //        catch (Exception e)
            //        {
            //            Console.WriteLine($"List Directory Error: {e}");
            //        }
            //    }

            //    Console.WriteLine($"Directory File Count: {results.Length}");
            //}

            //directory   "/econolite/set1"   string


            //List<Signal> _signalList;
            //var fileList = new List<FileInfo>();

            //using (var scope = host.Services.CreateScope())
            //{
            //    _signalList = scope.ServiceProvider.GetService<ISignalRepository>().GetLatestVersionOfAllSignals().Where(w => w.ControllerType.ControllerTypeId == 1).Take(25).ToList();

            //    foreach (var s in _signalList)
            //    {
            //        Console.WriteLine($"trying to download: {s.SignalId} | {s.ControllerType.ControllerTypeId}");

            //        try
            //        {
            //            var downloaders = scope.ServiceProvider.GetServices<ISignalControllerDownloader>();
            //            var downloader = downloaders.First(c => c.CanExecute(s));

            //            await foreach (var file in downloader.Execute(s, default))
            //            {
            //                Console.WriteLine($"downloaded file: {file.FullName}");

            //                fileList.Add(file);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine($"--------------------------------------------signalDownload catch: {ex}");
            //        }
            //    }
            //}


            //Console.WriteLine($"file count: {fileList.Count}");



            Console.ReadKey();
        }
    }
}