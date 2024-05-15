using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Services;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.ConfigurationModels;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.DownloaderClients;
using Google.Cloud.Diagnostics.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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




                    //s.AddTransient<IEmailService, SendGridEmailService>();
                    //s.AddTransient<IEmailService, SmtpEmailService>();

                    s.AddEmailServices(h);
                    




                    s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o =>
                    {
                        o.LocalPath = "C:\\temp2";
                        o.PingControllerToVerify = false;
                        o.ConnectionTimeout = 3000;
                        o.ReadTimeout = 3000;
                        o.DeleteFile = false;
                    });

                    //s.PostConfigureAll<EmailConfiguration>(o =>
                    //{
                    //    o.Host = "smtp.sendgrid.net";
                    //    o.Port = 587;
                    //    o.EnableSsl = false;
                    //    o.UserName = "apikey";
                    //    o.Password = ***REMOVED***;
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

            //host.Services.PrintHostInformation();

            //await host.RunAsync();
            //await host.StartAsync();
            //await host.StopAsync();


            //using (var scope = host.Services.CreateScope())
            //{
            //    var db = scope.ServiceProvider.GetService<ConfigContext>();

            //    var region = new Region() { Description = "test region" };
            //    var jurisdiction = new Jurisdiction() { Name = "test jurisdiction" };
            //    var locType = new LocationType() { Name = "test location type" };

            //    var r = db.Add(region);
            //    var j = db.Add(jurisdiction);
            //    var l = db.Add(locType);

            //    await db.SaveChangesAsync();

            //    var loc = new Location()
            //    {
            //        LocationIdentifier = "1234",
            //        PrimaryName = "primary",
            //        SecondaryName = "secondary",
            //        VersionAction = Data.Enums.LocationVersionActions.Initial,
            //        Longitude = 123,
            //        Latitude = 123,
            //        Start = DateTime.Now,
            //        RegionId = r.Entity.Id,
            //        JurisdictionId = j.Entity.Id,
            //        LocationTypeId = l.Entity.Id,
            //    };

            //    db.Add(loc);
            //    await db.SaveChangesAsync();
            //}

            using (var scope = host.Services.CreateScope())
            {
                //var db = scope.ServiceProvider.GetService<ConfigContext>();

                //var test = db.ChangeTracker.DebugView.ShortView;

                //var r = db.Regions.Include(i => i.Locations).FirstOrDefault();
                //db.Remove(r);

                //var j = db.Jurisdictions.Include(i => i.Locations).FirstOrDefault();
                //db.Remove(j);

                //await db.SaveChangesAsync();

                var regions = scope.ServiceProvider.GetService<IRegionsRepository>();
                var r = regions.Lookup(regions.GetList().FirstOrDefault());
                regions.Remove(r);

                var jurisdictions = scope.ServiceProvider.GetService<IJurisdictionRepository>();
                var j = jurisdictions.Lookup(jurisdictions.GetList().FirstOrDefault());
                jurisdictions.Remove(j);

            }

            Console.ReadLine();
        }
    }
}