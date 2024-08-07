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

using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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


                    s.AddDownloaderClients();

                    s.AddDeviceDownloaders(h);

                    s.AddEventLogDecoders();

                    s.AddEventLogImporters(h);

                    s.AddEmailServices(h);


                    //controller logger configuration
                    s.Configure<LocationControllerLoggerConfiguration>(h.Configuration.GetSection(nameof(LocationControllerLoggerConfiguration)));













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
                .ConfigureAppConfiguration((h, c) => c.AddCommandLine(args))

                //.UseConsoleLifetime()
                .Build();

            host.Services.PrintHostInformation();

            //await host.RunAsync();
            //await host.StartAsync();
            //await host.StopAsync();

            //using (var scope = host.Services.CreateScope())
            //{
            //    //Console.WriteLine($"{scope.ServiceProvider.GetService<IDownloaderClient>()}");
            //    //Console.WriteLine($"{scope.ServiceProvider.GetService<IFTPDownloaderClient>()}");
            //    //Console.WriteLine($"{scope.ServiceProvider.GetService<ISFTPDownloaderClient>()}");
            //    //Console.WriteLine($"{scope.ServiceProvider.GetService<IHTTPDownloaderClient>()}");

            //    foreach (var s in scope.ServiceProvider.GetServices<IDownloaderClient>())
            //    {
            //        Console.WriteLine($"----------------{s}");
            //    }

            //    foreach (var s in scope.ServiceProvider.GetServices<IDeviceDownloader>())
            //    {
            //        Console.WriteLine($"___________________{s}");
            //    }

            //    var devices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
            //       .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
            //       .Where(w => w.Ipaddress.IsValidIPAddress());
            //       //.OrderBy(o => o.Ipaddress)
            //       //.Take(10);

            //    Console.WriteLine($"devices: {devices.Count()}");

            //    foreach (var d in devices)
            //    {
            //        var downloader = scope.ServiceProvider.GetService<IDeviceDownloader>();

            //        try
            //        {
            //            var result = downloader.Execute(d);

            //            await foreach (var r in result)
            //            {
            //                try
            //                {
            //                    var data = File.ReadAllBytes(r.Item2.FullName);

            //                    //var info = $"{r.Item2.Extension}|{BitConverter.ToString(data.Take(25).ToArray())}|{Encoding.UTF8.GetString(data.Take(25).ToArray())}\n";
            //                    var info = $"{r.Item2.Extension}|{BitConverter.ToString(data.Take(25).ToArray())}\n";

            //                    info = info.Replace(",", "").Replace("|", ",").Replace("-", ",");

            //                    Console.WriteLine(info);

            //                    File.AppendAllText(Path.Combine("C:\\temp", "test.csv"), info);
            //                }
            //                catch (Exception e)
            //                {
            //                    Console.WriteLine($"error writing {r.Item2.FullName} - {e}");
            //                }
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            Console.WriteLine($"error downloading {d} - {e}");
            //        }
            //    }

            //}

            //using (var scope = host.Services.CreateScope())
            //{
            //    var email = scope.ServiceProvider.GetService<IEmailService>();

            //    var msg = new MailMessage("AtspmWatchdog@utah.gov", "christianbaker@utah.gov", "test email", "this is a test");

            //    await email.SendEmailAsync(msg);
            //}


            //var files = new DirectoryInfo("C:\\temp\\4006 - 5600 South \\SignalController\\10.202.19.143");
            //var device = new Device()
            //{
            //    Location = new Location() { LocationIdentifier = "4006"},
            //    DeviceConfiguration = new DeviceConfiguration() { Decoders = new[] { nameof(AscToIndianaDecoder)}}
            //};


            //foreach (var f in files.GetFiles())
            //{
            //    using (var scope = host.Services.CreateScope())
            //    {
            //        var importer = scope.ServiceProvider.GetService<IEventLogImporter>();

            //        var results = importer.Execute(Tuple.Create(device, f));

            //        await foreach (var r in results)
            //        {
            //            Console.WriteLine($"{r.Item2.GetType().Name} -- {r.Item2}");
            //        }
            //    }
            //}




            Console.WriteLine($"------------------done-----------------------");

            Console.ReadLine();
        }
    }
}