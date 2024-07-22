#region license
// Copyright 2024 Utah Departement of Transportation
// for LocationControllerLogger - ATSPM.LocationControllerLogger/Program.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Services;
using ATSPM.Data.Enums;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Extensions;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ATSPM.LocationControllerLogger
{
    class Program
    {
        static async Task Main(string[] args)
        {

            //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((h, l) =>
                {
                    //l.SetMinimumLevel(LogLevel.None);

                    //TODO: add a GoogleLogger section
                    //LoggingServiceOptions GoogleOptions = h.Configuration.GetSection("GoogleLogging").Get<LoggingServiceOptions>();
                    //TODO: remove this to an extension method
                    //DOTNET_ENVIRONMENT = Development,GOOGLE_APPLICATION_CREDENTIALS = M:\My Drive\ut-udot-atspm-dev-023438451801.json
                    //if (h.Configuration.GetValue<bool>("UseGoogleLogger"))
                    //{

                    //https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Diagnostics.Common/latest

                    l.AddGoogle(new LoggingServiceOptions
                    {
                        ProjectId = "1022556126938",
                        //ProjectId = "869261868126",
                        ServiceName = AppDomain.CurrentDomain.FriendlyName,
                        Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                        Options = LoggingOptions.Create(LogLevel.Information, AppDomain.CurrentDomain.FriendlyName)
                    });
                    //}
                })
                .ConfigureServices((h, s) =>
                {
                    //s.AddGoogleErrorReporting(new ErrorReportingServiceOptions() {
                    //    ProjectId = "1022556126938",
                    //    ServiceName = "ErrorReporting",
                    //    Version = "1.1",
                    //});

                    s.AddLogging();

                    s.AddAtspmDbContext(h);

                    //background services
                    //s.AddHostedService<LoggerBackgroundService>();

                    //repositories
                    s.AddAtspmEFConfigRepositories();
                    s.AddAtspmEFEventLogRepositories();
                    s.AddAtspmEFAggregationRepositories();

                    //s.AddTransient<IFileTranscoder, JsonFileTranscoder>();
                    //s.AddTransient<IFileTranscoder, ParquetFileTranscoder>();
                    //s.AddTransient<IFileTranscoder, CompressedJsonFileTranscoder>();

                    ////downloader clients
                    s.AddDownloaderClients();
                    //s.AddTransient<IHTTPDownloaderClient, HttpDownloaderClient>();
                    //s.AddTransient<IFTPDownloaderClient, FluentFTPDownloaderClient>();
                    //s.AddTransient<ISFTPDownloaderClient, SSHNetSFTPDownloaderClient>();

                    //downloaders
                    //s.AddScoped<IDeviceDownloader, DeviceFtpDownloader>();
                    //s.AddScoped<IDeviceDownloader, DeviceSftpDownloader>();
                    //s.AddScoped<IDeviceDownloader, DeviceHttpDownloader>();
                    ////s.AddScoped<IDeviceDownloader, DeviceSnmpDownloader>();
                    s.AddDeviceDownloaders(h);

                    //decoders
                    s.AddScoped<IEventLogDecoder<IndianaEvent>, ASCEventLogDecoder>();
                    //s.AddScoped<IEventLogDecoder<IndianaEvent>, MaxTimeEventLogDecoder>();

                    //LocationControllerDataFlow
                    //s.AddScoped<ILocationControllerLoggerService, CompressedLocationControllerLogger>();
                    //s.AddScoped<ILocationControllerLoggerService, LegacyLocationControllerLogger>();

                    //controller logger configuration
                    s.Configure<LocationControllerLoggerConfiguration>(h.Configuration.GetSection(nameof(LocationControllerLoggerConfiguration)));

                    //downloader configurations
                    //s.ConfigureSignalControllerDownloaders(h);
                    //s.Configure<DeviceDownloaderConfiguration>(nameof(DeviceFtpDownloader), h.Configuration.GetSection($"{nameof(DeviceDownloaderConfiguration)}:{nameof(DeviceFtpDownloader)}"));
                    //s.Configure<DeviceDownloaderConfiguration>(nameof(CobaltLocationControllerDownloader), h.Configuration.GetSection($"{nameof(DeviceDownloaderConfiguration)}:{nameof(CobaltLocationControllerDownloader)}"));
                    //s.Configure<DeviceDownloaderConfiguration>(nameof(MaxTimeLocationControllerDownloader), h.Configuration.GetSection($"{nameof(DeviceDownloaderConfiguration)}:{nameof(MaxTimeLocationControllerDownloader)}"));
                    //s.Configure<DeviceDownloaderConfiguration>(nameof(EOSSignalControllerDownloader), h.Configuration.GetSection($"{nameof(DeviceDownloaderConfiguration)}:{nameof(EOSSignalControllerDownloader)}"));
                    //s.Configure<DeviceDownloaderConfiguration>(nameof(NewCobaltLocationControllerDownloader), h.Configuration.GetSection($"{nameof(DeviceDownloaderConfiguration)}:{nameof(NewCobaltLocationControllerDownloader)}"));

                    //decoder configurations
                    //s.Configure<SignalControllerDecoderConfiguration>(nameof(ASCEventLogDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(ASCEventLogDecoder)}"));
                    //s.Configure<SignalControllerDecoderConfiguration>(nameof(MaxTimeEventLogDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(MaxTimeEventLogDecoder)}"));

                    //s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));























                    //s.AddHostedService<TestService>();





                    s.AddTransient<ServiceTest>();







                    s.AddEmailServices(h);
                    

                    


                    s.PostConfigureAll<DeviceDownloaderConfiguration>(o =>
                    {
                        o.LocalPath = "C:\\temp3";
                        o.Ping = true;
                        //o.ConnectionTimeout = 2000;
                        //o.OperationTimeout = 30000;
                        o.DeleteFile = false;
                    });

                    //s.PostConfigureAll<EmailConfiguration>(o =>
                    //{
                    //    o.Host = "smtp.sendgrid.net";
                    //    o.Port = 587;
                    //    o.EnableSsl = false;
                    //    o.UserName = "apikey";
                    //    o.Password = "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";
                    //});

                    //              "DefaultEmailAddress": "dlowe@avenueconsultants.com",
                    //"EmailAllErrors": false,
                    //"EmailType": "smtp",
                    //"SmtpSettings": {
                    //              "Host": "smtp-relay.brevo.com",
                    //  "Port": 587,
                    //  "EnableSsl": true,
                    //  "UserName": "dlowe@avenueconsultants.com",
                    //  "Password": "Bb1SkPtsE5hLQYn4"
                    //},
                })

                //.UseConsoleLifetime()
                .Build();

            host.Services.PrintHostInformation();

            //await host.RunAsync();
            //await host.StartAsync();
            //await host.StopAsync();

            using (var scope = host.Services.CreateScope())
            {
                //Console.WriteLine($"{scope.ServiceProvider.GetService<IDownloaderClient>()}");
                //Console.WriteLine($"{scope.ServiceProvider.GetService<IFTPDownloaderClient>()}");
                //Console.WriteLine($"{scope.ServiceProvider.GetService<ISFTPDownloaderClient>()}");
                //Console.WriteLine($"{scope.ServiceProvider.GetService<IHTTPDownloaderClient>()}");

                foreach (var s in scope.ServiceProvider.GetServices<IDownloaderClient>())
                {
                    Console.WriteLine($"----------------{s}");
                }

                foreach (var s in scope.ServiceProvider.GetServices<IDeviceDownloader>())
                {
                    Console.WriteLine($"___________________{s}");
                }

                var devices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
                   .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
                   .Where(w => w.Ipaddress.IsValidIPAddress())
                   .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Http);
                   //.OrderBy(o => o.Ipaddress)
                   //.Take(10);

                Console.WriteLine($"devices: {devices.Count()}");

                foreach (var d in devices)
                {
                    var downloader = scope.ServiceProvider.GetService<IDeviceDownloader>();

                    try
                    {
                        var result = downloader.Execute(d);

                        await foreach (var r in result)
                        {
                            Console.WriteLine($"{r.Item1} - {r.Item2.FullName}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{d} - {e}");
                    }
                }

            }

            Console.WriteLine($"------------------done-----------------------");

            Console.ReadLine();
        }
    }

    public class ServiceTest
    {
        public ServiceTest(IEnumerable<IDownloaderClient> downloaderClients)
        {
            foreach (var o in downloaderClients)
            {
                Console.WriteLine($"------------------{o}");
            }
        }
    }
}