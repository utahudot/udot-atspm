using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.Services;
using ATSPM.Data;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Workflows;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.DownloaderClients;
using AutoFixture;
using Google.Cloud.Diagnostics.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Index.HPRtree;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.LocationControllerLogger
{
    class Program
    {
        static async Task Main(string[] args)
        {

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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
                    s.AddTransient<IHTTPDownloaderClient, HttpDownloaderClient>();
                    s.AddTransient<IFTPDownloaderClient, FluentFTPDownloaderClient>();
                    s.AddTransient<ISFTPDownloaderClient, SSHNetSFTPDownloaderClient>();

                    //downloaders
                    //s.AddScoped<IDeviceDownloader, DeviceFtpDownloader>();
                    //s.AddScoped<IDeviceDownloader, CobaltLocationControllerDownloader>();
                    //s.AddScoped<IDeviceDownloader, MaxTimeLocationControllerDownloader>();
                    //s.AddScoped<IDeviceDownloader, EOSSignalControllerDownloader>();
                    //s.AddScoped<IDeviceDownloader, NewCobaltLocationControllerDownloader>();
                    s.AddScoped<IDeviceDownloader, DeviceFtpDownloader>();
                    s.AddScoped<IDeviceDownloader, DeviceSftpDownloader>();
                    s.AddScoped<IDeviceDownloader, DeviceHttpDownloader>();
                    //s.AddScoped<IDeviceDownloader, DeviceSnmpDownloader>();

                    //decoders
                    s.AddScoped<ILocationControllerDecoder<IndianaEvent>, ASCLocationControllerDecoder>();
                    //s.AddScoped<ILocationControllerDecoder<IndianaEvent>, MaxTimeLocationControllerDecoder>();

                    //LocationControllerDataFlow
                    //s.AddScoped<ILocationControllerLoggerService, CompressedLocationControllerLogger>();
                    //s.AddScoped<ILocationControllerLoggerService, LegacyLocationControllerLogger>();

                    //controller logger configuration
                    s.Configure<LocationControllerLoggerConfiguration>(h.Configuration.GetSection(nameof(LocationControllerLoggerConfiguration)));

                    //downloader configurations
                    //s.ConfigureSignalControllerDownloaders(h);
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(DeviceFtpDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(DeviceFtpDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(CobaltLocationControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(CobaltLocationControllerDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(MaxTimeLocationControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(MaxTimeLocationControllerDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(EOSSignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(EOSSignalControllerDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(NewCobaltLocationControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(NewCobaltLocationControllerDownloader)}"));

                    //decoder configurations
                    //s.Configure<SignalControllerDecoderConfiguration>(nameof(ASCLocationControllerDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(ASCLocationControllerDecoder)}"));
                    //s.Configure<SignalControllerDecoderConfiguration>(nameof(MaxTimeLocationControllerDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(MaxTimeLocationControllerDecoder)}"));

                    //s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));

                    s.AddHostedService<TestService>();



                    s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o =>
                    {
                        o.LocalPath = "C:\\temp2";
                        o.PingControllerToVerify = false;
                        o.ConnectionTimeout = 3000;
                        o.ReadTimeout = 3000;
                        o.DeleteFile = false;
                    });

                    
                })

                //.UseConsoleLifetime()
                .Build();

            //host.Services.PrintHostInformation();

            //await host.RunAsync();
            //await host.StartAsync();
            //await host.StopAsync();

            var json = File.ReadAllText(new FileInfo(@"M:\My Drive\Controller Protocols\QFree Enumerations\event_codes.json").FullName);
            Root root = JsonConvert.DeserializeObject<Root>(json);

            List<QFree> qfree = new();

            foreach (var p in root.GetType().GetProperties())
            {
                //-t   { ATSPM.LocationControllerLogger._0}
                //object { ATSPM.LocationControllerLogger._0}

                dynamic t = p.GetValue(root, null);

                string test = t.GetType().Name;
                int code = int.Parse(test.Substring(1, test.Length - 1));

                //Console.WriteLine($"code: {code}");

                qfree.Add(new QFree()
                {
                    Code = code,
                    Name = t.Name,
                    Description = t.Description,
                    ParameterDescription = t.ParameterDescription
                });
            }

            var lines = qfree.Select(s => $"{s.Code},{s.Name},{s.Description.Replace(',', '|')},{s.ParameterDescription.Replace(',', '|')}").ToList();
            lines.Insert(0,"Code,Name,Description,ParameterDescription");

            var csv = string.Join("\n", lines);

            Console.WriteLine($"{csv}");

            File.WriteAllText(@"M:\My Drive\Controller Protocols\QFree Enumerations\codes.csv", csv);

            Console.ReadLine();
        }
    }

    public class QFree
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class _0
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _1
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _10
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _101
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _102
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _103
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _104
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _105
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _106
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _107
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _108
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _109
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _11
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _110
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _111
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _112
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _113
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _114
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _115
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _116
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _117
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _118
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _119
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _12
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _13
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _131
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _132
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _133
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _134
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _135
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _136
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _137
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _138
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _139
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _14
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _140
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _141
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _142
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _143
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _144
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _145
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _146
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _147
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _148
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _149
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _15
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _150
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _151
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _152
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _153
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _154
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _155
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _156
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _16
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _17
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _171
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _172
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _173
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _174
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _175
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _176
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _177
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _178
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _179
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _18
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _180
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _181
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _182
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _183
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _184
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _185
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _19
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _2
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _20
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _200
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _201
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _202
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _203
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _204
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _205
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _206
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _207
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _208
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _209
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _21
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _210
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _211
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _212
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _213
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _214
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _215
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _216
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _217
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _218
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _22
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _23
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _24
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _25
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _26
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _3
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _300
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _301
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _302
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _303
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _304
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _305
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _306
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _307
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _308
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _309
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _31
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _310
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _311
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _312
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _313
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _314
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _315
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _316
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _317
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _318
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _319
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _32
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _320
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _321
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _322
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _323
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _324
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _325
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _326
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _327
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _328
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _329
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _33
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _330
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _331
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _332
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _333
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _334
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _335
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _336
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _337
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _338
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _339
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _340
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _341
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _342
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _343
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _344
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _345
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _346
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _347
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _348
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _349
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _350
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _351
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _352
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _353
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _354
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _355
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _356
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _357
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _358
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _359
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _360
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _361
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _362
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _363
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _364
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _365
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _366
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _367
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _368
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _369
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _370
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _371
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _4
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _400
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _41
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _42
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _43
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _44
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _45
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _46
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _47
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _48
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _49
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _5
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _50
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _500
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _501
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _502
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _503
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _51
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _517
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _518
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _519
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _52
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _520
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _521
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _53
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _54
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _55
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _56
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _6
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _600
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _601
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _602
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _603
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _604
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _605
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _606
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _607
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _608
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _609
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _61
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _610
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _611
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _612
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _613
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _614
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _615
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _616
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _617
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _618
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _619
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _62
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _620
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _621
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _622
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _623
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _624
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _625
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _626
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _627
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _628
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _629
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _63
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _630
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _631
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _632
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _633
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _634
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _635
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _636
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _637
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _638
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _639
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _64
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _640
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _641
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _642
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _643
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _644
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _645
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _646
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _647
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _648
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _649
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _65
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _650
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _651
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _66
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _67
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _68
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _69
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _7
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _70
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _701
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _702
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _703
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _704
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _705
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _706
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _707
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _708
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _709
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _71
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _710
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _711
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _712
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _713
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _714
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _715
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _716
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _717
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _718
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _719
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _72
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _720
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _721
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _8
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _81
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _82
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _83
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _84
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _85
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _86
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _87
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _88
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _89
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _9
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _90
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _900
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _901
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _902
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _903
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _904
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _905
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _906
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _907
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _908
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _909
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _91
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _910
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _911
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _912
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _913
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _914
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _915
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _916
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _92
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _93
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class _94
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterDescription { get; set; }
    }

    public class Root
    {
        [JsonProperty("0")]
        public _0 _0 { get; set; }

        [JsonProperty("1")]
        public _1 _1 { get; set; }

        [JsonProperty("2")]
        public _2 _2 { get; set; }

        [JsonProperty("3")]
        public _3 _3 { get; set; }

        [JsonProperty("4")]
        public _4 _4 { get; set; }

        [JsonProperty("5")]
        public _5 _5 { get; set; }

        [JsonProperty("6")]
        public _6 _6 { get; set; }

        [JsonProperty("7")]
        public _7 _7 { get; set; }

        [JsonProperty("8")]
        public _8 _8 { get; set; }

        [JsonProperty("9")]
        public _9 _9 { get; set; }

        [JsonProperty("10")]
        public _10 _10 { get; set; }

        [JsonProperty("11")]
        public _11 _11 { get; set; }

        [JsonProperty("12")]
        public _12 _12 { get; set; }

        [JsonProperty("13")]
        public _13 _13 { get; set; }

        [JsonProperty("14")]
        public _14 _14 { get; set; }

        [JsonProperty("15")]
        public _15 _15 { get; set; }

        [JsonProperty("16")]
        public _16 _16 { get; set; }

        [JsonProperty("17")]
        public _17 _17 { get; set; }

        [JsonProperty("18")]
        public _18 _18 { get; set; }

        [JsonProperty("19")]
        public _19 _19 { get; set; }

        [JsonProperty("20")]
        public _20 _20 { get; set; }

        [JsonProperty("21")]
        public _21 _21 { get; set; }

        [JsonProperty("22")]
        public _22 _22 { get; set; }

        [JsonProperty("23")]
        public _23 _23 { get; set; }

        [JsonProperty("24")]
        public _24 _24 { get; set; }

        [JsonProperty("25")]
        public _25 _25 { get; set; }

        [JsonProperty("26")]
        public _26 _26 { get; set; }

        [JsonProperty("31")]
        public _31 _31 { get; set; }

        [JsonProperty("32")]
        public _32 _32 { get; set; }

        [JsonProperty("33")]
        public _33 _33 { get; set; }

        [JsonProperty("41")]
        public _41 _41 { get; set; }

        [JsonProperty("42")]
        public _42 _42 { get; set; }

        [JsonProperty("43")]
        public _43 _43 { get; set; }

        [JsonProperty("44")]
        public _44 _44 { get; set; }

        [JsonProperty("45")]
        public _45 _45 { get; set; }

        [JsonProperty("46")]
        public _46 _46 { get; set; }

        [JsonProperty("47")]
        public _47 _47 { get; set; }

        [JsonProperty("48")]
        public _48 _48 { get; set; }

        [JsonProperty("49")]
        public _49 _49 { get; set; }

        [JsonProperty("50")]
        public _50 _50 { get; set; }

        [JsonProperty("51")]
        public _51 _51 { get; set; }

        [JsonProperty("52")]
        public _52 _52 { get; set; }

        [JsonProperty("53")]
        public _53 _53 { get; set; }

        [JsonProperty("54")]
        public _54 _54 { get; set; }

        [JsonProperty("55")]
        public _55 _55 { get; set; }

        [JsonProperty("56")]
        public _56 _56 { get; set; }

        [JsonProperty("61")]
        public _61 _61 { get; set; }

        [JsonProperty("62")]
        public _62 _62 { get; set; }

        [JsonProperty("63")]
        public _63 _63 { get; set; }

        [JsonProperty("64")]
        public _64 _64 { get; set; }

        [JsonProperty("65")]
        public _65 _65 { get; set; }

        [JsonProperty("66")]
        public _66 _66 { get; set; }

        [JsonProperty("67")]
        public _67 _67 { get; set; }

        [JsonProperty("68")]
        public _68 _68 { get; set; }

        [JsonProperty("69")]
        public _69 _69 { get; set; }

        [JsonProperty("70")]
        public _70 _70 { get; set; }

        [JsonProperty("71")]
        public _71 _71 { get; set; }

        [JsonProperty("72")]
        public _72 _72 { get; set; }

        [JsonProperty("81")]
        public _81 _81 { get; set; }

        [JsonProperty("82")]
        public _82 _82 { get; set; }

        [JsonProperty("83")]
        public _83 _83 { get; set; }

        [JsonProperty("84")]
        public _84 _84 { get; set; }

        [JsonProperty("85")]
        public _85 _85 { get; set; }

        [JsonProperty("86")]
        public _86 _86 { get; set; }

        [JsonProperty("87")]
        public _87 _87 { get; set; }

        [JsonProperty("88")]
        public _88 _88 { get; set; }

        [JsonProperty("89")]
        public _89 _89 { get; set; }

        [JsonProperty("90")]
        public _90 _90 { get; set; }

        [JsonProperty("91")]
        public _91 _91 { get; set; }

        [JsonProperty("92")]
        public _92 _92 { get; set; }

        [JsonProperty("93")]
        public _93 _93 { get; set; }

        [JsonProperty("94")]
        public _94 _94 { get; set; }

        [JsonProperty("101")]
        public _101 _101 { get; set; }

        [JsonProperty("102")]
        public _102 _102 { get; set; }

        [JsonProperty("103")]
        public _103 _103 { get; set; }

        [JsonProperty("104")]
        public _104 _104 { get; set; }

        [JsonProperty("105")]
        public _105 _105 { get; set; }

        [JsonProperty("106")]
        public _106 _106 { get; set; }

        [JsonProperty("107")]
        public _107 _107 { get; set; }

        [JsonProperty("108")]
        public _108 _108 { get; set; }

        [JsonProperty("109")]
        public _109 _109 { get; set; }

        [JsonProperty("110")]
        public _110 _110 { get; set; }

        [JsonProperty("111")]
        public _111 _111 { get; set; }

        [JsonProperty("112")]
        public _112 _112 { get; set; }

        [JsonProperty("113")]
        public _113 _113 { get; set; }

        [JsonProperty("114")]
        public _114 _114 { get; set; }

        [JsonProperty("115")]
        public _115 _115 { get; set; }

        [JsonProperty("116")]
        public _116 _116 { get; set; }

        [JsonProperty("117")]
        public _117 _117 { get; set; }

        [JsonProperty("118")]
        public _118 _118 { get; set; }

        [JsonProperty("119")]
        public _119 _119 { get; set; }

        [JsonProperty("131")]
        public _131 _131 { get; set; }

        [JsonProperty("132")]
        public _132 _132 { get; set; }

        [JsonProperty("133")]
        public _133 _133 { get; set; }

        [JsonProperty("134")]
        public _134 _134 { get; set; }

        [JsonProperty("135")]
        public _135 _135 { get; set; }

        [JsonProperty("136")]
        public _136 _136 { get; set; }

        [JsonProperty("137")]
        public _137 _137 { get; set; }

        [JsonProperty("138")]
        public _138 _138 { get; set; }

        [JsonProperty("139")]
        public _139 _139 { get; set; }

        [JsonProperty("140")]
        public _140 _140 { get; set; }

        [JsonProperty("141")]
        public _141 _141 { get; set; }

        [JsonProperty("142")]
        public _142 _142 { get; set; }

        [JsonProperty("143")]
        public _143 _143 { get; set; }

        [JsonProperty("144")]
        public _144 _144 { get; set; }

        [JsonProperty("145")]
        public _145 _145 { get; set; }

        [JsonProperty("146")]
        public _146 _146 { get; set; }

        [JsonProperty("147")]
        public _147 _147 { get; set; }

        [JsonProperty("148")]
        public _148 _148 { get; set; }

        [JsonProperty("149")]
        public _149 _149 { get; set; }

        [JsonProperty("150")]
        public _150 _150 { get; set; }

        [JsonProperty("151")]
        public _151 _151 { get; set; }

        [JsonProperty("152")]
        public _152 _152 { get; set; }

        [JsonProperty("153")]
        public _153 _153 { get; set; }

        [JsonProperty("154")]
        public _154 _154 { get; set; }

        [JsonProperty("155")]
        public _155 _155 { get; set; }

        [JsonProperty("156")]
        public _156 _156 { get; set; }

        [JsonProperty("171")]
        public _171 _171 { get; set; }

        [JsonProperty("172")]
        public _172 _172 { get; set; }

        [JsonProperty("173")]
        public _173 _173 { get; set; }

        [JsonProperty("174")]
        public _174 _174 { get; set; }

        [JsonProperty("175")]
        public _175 _175 { get; set; }

        [JsonProperty("176")]
        public _176 _176 { get; set; }

        [JsonProperty("177")]
        public _177 _177 { get; set; }

        [JsonProperty("178")]
        public _178 _178 { get; set; }

        [JsonProperty("179")]
        public _179 _179 { get; set; }

        [JsonProperty("180")]
        public _180 _180 { get; set; }

        [JsonProperty("181")]
        public _181 _181 { get; set; }

        [JsonProperty("182")]
        public _182 _182 { get; set; }

        [JsonProperty("183")]
        public _183 _183 { get; set; }

        [JsonProperty("184")]
        public _184 _184 { get; set; }

        [JsonProperty("185")]
        public _185 _185 { get; set; }

        [JsonProperty("200")]
        public _200 _200 { get; set; }

        [JsonProperty("201")]
        public _201 _201 { get; set; }

        [JsonProperty("202")]
        public _202 _202 { get; set; }

        [JsonProperty("203")]
        public _203 _203 { get; set; }

        [JsonProperty("204")]
        public _204 _204 { get; set; }

        [JsonProperty("205")]
        public _205 _205 { get; set; }

        [JsonProperty("206")]
        public _206 _206 { get; set; }

        [JsonProperty("207")]
        public _207 _207 { get; set; }

        [JsonProperty("208")]
        public _208 _208 { get; set; }

        [JsonProperty("209")]
        public _209 _209 { get; set; }

        [JsonProperty("210")]
        public _210 _210 { get; set; }

        [JsonProperty("211")]
        public _211 _211 { get; set; }

        [JsonProperty("212")]
        public _212 _212 { get; set; }

        [JsonProperty("213")]
        public _213 _213 { get; set; }

        [JsonProperty("214")]
        public _214 _214 { get; set; }

        [JsonProperty("215")]
        public _215 _215 { get; set; }

        [JsonProperty("216")]
        public _216 _216 { get; set; }

        [JsonProperty("217")]
        public _217 _217 { get; set; }

        [JsonProperty("218")]
        public _218 _218 { get; set; }

        [JsonProperty("300")]
        public _300 _300 { get; set; }

        [JsonProperty("301")]
        public _301 _301 { get; set; }

        [JsonProperty("302")]
        public _302 _302 { get; set; }

        [JsonProperty("303")]
        public _303 _303 { get; set; }

        [JsonProperty("304")]
        public _304 _304 { get; set; }

        [JsonProperty("305")]
        public _305 _305 { get; set; }

        [JsonProperty("306")]
        public _306 _306 { get; set; }

        [JsonProperty("307")]
        public _307 _307 { get; set; }

        [JsonProperty("308")]
        public _308 _308 { get; set; }

        [JsonProperty("309")]
        public _309 _309 { get; set; }

        [JsonProperty("310")]
        public _310 _310 { get; set; }

        [JsonProperty("311")]
        public _311 _311 { get; set; }

        [JsonProperty("312")]
        public _312 _312 { get; set; }

        [JsonProperty("313")]
        public _313 _313 { get; set; }

        [JsonProperty("314")]
        public _314 _314 { get; set; }

        [JsonProperty("315")]
        public _315 _315 { get; set; }

        [JsonProperty("316")]
        public _316 _316 { get; set; }

        [JsonProperty("317")]
        public _317 _317 { get; set; }

        [JsonProperty("318")]
        public _318 _318 { get; set; }

        [JsonProperty("319")]
        public _319 _319 { get; set; }

        [JsonProperty("320")]
        public _320 _320 { get; set; }

        [JsonProperty("321")]
        public _321 _321 { get; set; }

        [JsonProperty("322")]
        public _322 _322 { get; set; }

        [JsonProperty("323")]
        public _323 _323 { get; set; }

        [JsonProperty("324")]
        public _324 _324 { get; set; }

        [JsonProperty("325")]
        public _325 _325 { get; set; }

        [JsonProperty("326")]
        public _326 _326 { get; set; }

        [JsonProperty("327")]
        public _327 _327 { get; set; }

        [JsonProperty("328")]
        public _328 _328 { get; set; }

        [JsonProperty("329")]
        public _329 _329 { get; set; }

        [JsonProperty("330")]
        public _330 _330 { get; set; }

        [JsonProperty("331")]
        public _331 _331 { get; set; }

        [JsonProperty("332")]
        public _332 _332 { get; set; }

        [JsonProperty("333")]
        public _333 _333 { get; set; }

        [JsonProperty("334")]
        public _334 _334 { get; set; }

        [JsonProperty("335")]
        public _335 _335 { get; set; }

        [JsonProperty("336")]
        public _336 _336 { get; set; }

        [JsonProperty("337")]
        public _337 _337 { get; set; }

        [JsonProperty("338")]
        public _338 _338 { get; set; }

        [JsonProperty("339")]
        public _339 _339 { get; set; }

        [JsonProperty("340")]
        public _340 _340 { get; set; }

        [JsonProperty("341")]
        public _341 _341 { get; set; }

        [JsonProperty("342")]
        public _342 _342 { get; set; }

        [JsonProperty("343")]
        public _343 _343 { get; set; }

        [JsonProperty("344")]
        public _344 _344 { get; set; }

        [JsonProperty("345")]
        public _345 _345 { get; set; }

        [JsonProperty("346")]
        public _346 _346 { get; set; }

        [JsonProperty("347")]
        public _347 _347 { get; set; }

        [JsonProperty("348")]
        public _348 _348 { get; set; }

        [JsonProperty("349")]
        public _349 _349 { get; set; }

        [JsonProperty("350")]
        public _350 _350 { get; set; }

        [JsonProperty("351")]
        public _351 _351 { get; set; }

        [JsonProperty("352")]
        public _352 _352 { get; set; }

        [JsonProperty("353")]
        public _353 _353 { get; set; }

        [JsonProperty("354")]
        public _354 _354 { get; set; }

        [JsonProperty("355")]
        public _355 _355 { get; set; }

        [JsonProperty("356")]
        public _356 _356 { get; set; }

        [JsonProperty("357")]
        public _357 _357 { get; set; }

        [JsonProperty("358")]
        public _358 _358 { get; set; }

        [JsonProperty("359")]
        public _359 _359 { get; set; }

        [JsonProperty("360")]
        public _360 _360 { get; set; }

        [JsonProperty("361")]
        public _361 _361 { get; set; }

        [JsonProperty("362")]
        public _362 _362 { get; set; }

        [JsonProperty("363")]
        public _363 _363 { get; set; }

        [JsonProperty("364")]
        public _364 _364 { get; set; }

        [JsonProperty("365")]
        public _365 _365 { get; set; }

        [JsonProperty("366")]
        public _366 _366 { get; set; }

        [JsonProperty("367")]
        public _367 _367 { get; set; }

        [JsonProperty("368")]
        public _368 _368 { get; set; }

        [JsonProperty("369")]
        public _369 _369 { get; set; }

        [JsonProperty("370")]
        public _370 _370 { get; set; }

        [JsonProperty("371")]
        public _371 _371 { get; set; }

        [JsonProperty("400")]
        public _400 _400 { get; set; }

        [JsonProperty("500")]
        public _500 _500 { get; set; }

        [JsonProperty("501")]
        public _501 _501 { get; set; }

        [JsonProperty("502")]
        public _502 _502 { get; set; }

        [JsonProperty("503")]
        public _503 _503 { get; set; }

        [JsonProperty("517")]
        public _517 _517 { get; set; }

        [JsonProperty("518")]
        public _518 _518 { get; set; }

        [JsonProperty("519")]
        public _519 _519 { get; set; }

        [JsonProperty("520")]
        public _520 _520 { get; set; }

        [JsonProperty("521")]
        public _521 _521 { get; set; }

        [JsonProperty("600")]
        public _600 _600 { get; set; }

        [JsonProperty("601")]
        public _601 _601 { get; set; }

        [JsonProperty("602")]
        public _602 _602 { get; set; }

        [JsonProperty("603")]
        public _603 _603 { get; set; }

        [JsonProperty("604")]
        public _604 _604 { get; set; }

        [JsonProperty("605")]
        public _605 _605 { get; set; }

        [JsonProperty("606")]
        public _606 _606 { get; set; }

        [JsonProperty("607")]
        public _607 _607 { get; set; }

        [JsonProperty("608")]
        public _608 _608 { get; set; }

        [JsonProperty("609")]
        public _609 _609 { get; set; }

        [JsonProperty("610")]
        public _610 _610 { get; set; }

        [JsonProperty("611")]
        public _611 _611 { get; set; }

        [JsonProperty("612")]
        public _612 _612 { get; set; }

        [JsonProperty("613")]
        public _613 _613 { get; set; }

        [JsonProperty("614")]
        public _614 _614 { get; set; }

        [JsonProperty("615")]
        public _615 _615 { get; set; }

        [JsonProperty("616")]
        public _616 _616 { get; set; }

        [JsonProperty("617")]
        public _617 _617 { get; set; }

        [JsonProperty("618")]
        public _618 _618 { get; set; }

        [JsonProperty("619")]
        public _619 _619 { get; set; }

        [JsonProperty("620")]
        public _620 _620 { get; set; }

        [JsonProperty("621")]
        public _621 _621 { get; set; }

        [JsonProperty("622")]
        public _622 _622 { get; set; }

        [JsonProperty("623")]
        public _623 _623 { get; set; }

        [JsonProperty("624")]
        public _624 _624 { get; set; }

        [JsonProperty("625")]
        public _625 _625 { get; set; }

        [JsonProperty("626")]
        public _626 _626 { get; set; }

        [JsonProperty("627")]
        public _627 _627 { get; set; }

        [JsonProperty("628")]
        public _628 _628 { get; set; }

        [JsonProperty("629")]
        public _629 _629 { get; set; }

        [JsonProperty("630")]
        public _630 _630 { get; set; }

        [JsonProperty("631")]
        public _631 _631 { get; set; }

        [JsonProperty("632")]
        public _632 _632 { get; set; }

        [JsonProperty("633")]
        public _633 _633 { get; set; }

        [JsonProperty("634")]
        public _634 _634 { get; set; }

        [JsonProperty("635")]
        public _635 _635 { get; set; }

        [JsonProperty("636")]
        public _636 _636 { get; set; }

        [JsonProperty("637")]
        public _637 _637 { get; set; }

        [JsonProperty("638")]
        public _638 _638 { get; set; }

        [JsonProperty("639")]
        public _639 _639 { get; set; }

        [JsonProperty("640")]
        public _640 _640 { get; set; }

        [JsonProperty("641")]
        public _641 _641 { get; set; }

        [JsonProperty("642")]
        public _642 _642 { get; set; }

        [JsonProperty("643")]
        public _643 _643 { get; set; }

        [JsonProperty("644")]
        public _644 _644 { get; set; }

        [JsonProperty("645")]
        public _645 _645 { get; set; }

        [JsonProperty("646")]
        public _646 _646 { get; set; }

        [JsonProperty("647")]
        public _647 _647 { get; set; }

        [JsonProperty("648")]
        public _648 _648 { get; set; }

        [JsonProperty("649")]
        public _649 _649 { get; set; }

        [JsonProperty("650")]
        public _650 _650 { get; set; }

        [JsonProperty("651")]
        public _651 _651 { get; set; }

        [JsonProperty("701")]
        public _701 _701 { get; set; }

        [JsonProperty("702")]
        public _702 _702 { get; set; }

        [JsonProperty("703")]
        public _703 _703 { get; set; }

        [JsonProperty("704")]
        public _704 _704 { get; set; }

        [JsonProperty("705")]
        public _705 _705 { get; set; }

        [JsonProperty("706")]
        public _706 _706 { get; set; }

        [JsonProperty("707")]
        public _707 _707 { get; set; }

        [JsonProperty("708")]
        public _708 _708 { get; set; }

        [JsonProperty("709")]
        public _709 _709 { get; set; }

        [JsonProperty("710")]
        public _710 _710 { get; set; }

        [JsonProperty("711")]
        public _711 _711 { get; set; }

        [JsonProperty("712")]
        public _712 _712 { get; set; }

        [JsonProperty("713")]
        public _713 _713 { get; set; }

        [JsonProperty("714")]
        public _714 _714 { get; set; }

        [JsonProperty("715")]
        public _715 _715 { get; set; }

        [JsonProperty("716")]
        public _716 _716 { get; set; }

        [JsonProperty("717")]
        public _717 _717 { get; set; }

        [JsonProperty("718")]
        public _718 _718 { get; set; }

        [JsonProperty("719")]
        public _719 _719 { get; set; }

        [JsonProperty("720")]
        public _720 _720 { get; set; }

        [JsonProperty("721")]
        public _721 _721 { get; set; }

        [JsonProperty("900")]
        public _900 _900 { get; set; }

        [JsonProperty("901")]
        public _901 _901 { get; set; }

        [JsonProperty("902")]
        public _902 _902 { get; set; }

        [JsonProperty("903")]
        public _903 _903 { get; set; }

        [JsonProperty("904")]
        public _904 _904 { get; set; }

        [JsonProperty("905")]
        public _905 _905 { get; set; }

        [JsonProperty("906")]
        public _906 _906 { get; set; }

        [JsonProperty("907")]
        public _907 _907 { get; set; }

        [JsonProperty("908")]
        public _908 _908 { get; set; }

        [JsonProperty("909")]
        public _909 _909 { get; set; }

        [JsonProperty("910")]
        public _910 _910 { get; set; }

        [JsonProperty("911")]
        public _911 _911 { get; set; }

        [JsonProperty("912")]
        public _912 _912 { get; set; }

        [JsonProperty("913")]
        public _913 _913 { get; set; }

        [JsonProperty("914")]
        public _914 _914 { get; set; }

        [JsonProperty("915")]
        public _915 _915 { get; set; }

        [JsonProperty("916")]
        public _916 _916 { get; set; }
    }


}