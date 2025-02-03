#region license
// Copyright 2024 Utah Departement of Transportation
// for EventLogUtility - %Namespace%/Program.cs
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

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Parquet.Data.Rows;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.EventLogUtility.Commands;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.Atspm.Infrastructure.Services.DeviceDownloaders;
using Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients;
using Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders;


//if (OperatingSystem.IsWindows())
//{
//    if (!EventLog.SourceExists("Atspm"))
//        EventLog.CreateEventSource(AppDomain.CurrentDomain.FriendlyName, "Atspm");
//}


//var rootCmd = new EventLogCommands();
//var cmdBuilder = new CommandLineBuilder(rootCmd);
//cmdBuilder.UseDefaults();
//cmdBuilder.UseHost(a =>
//{
//    return Host.CreateDefaultBuilder(a)
//    //.UseConsoleLifetime()
//    .ApplyVolumeConfiguration()
//    .ConfigureLogging((h, l) =>
//    {
//        if (OperatingSystem.IsWindows())
//        {
//            l.AddEventLog(c =>
//            {
//                c.SourceName = AppDomain.CurrentDomain.FriendlyName;
//                c.LogName = "Atspm";
//            });
//        }


//        //l.AddGoogle(new LoggingServiceOptions
//        //{
//        //    //ProjectId = "",
//        //    ServiceName = AppDomain.CurrentDomain.FriendlyName,
//        //    Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
//        //    Options = WatchdogLoggingOptions.Create(LogLevel.Debug, AppDomain.CurrentDomain.FriendlyName)
//        //});
//    })
//    .ConfigureServices((h, s) =>
//    {
//        s.AddAtspmDbContext(h);
//        s.AddAtspmEFConfigRepositories();
//        s.AddAtspmEFEventLogRepositories();
//        s.AddAtspmEFAggregationRepositories();
//        s.AddDownloaderClients();
//        s.AddDeviceDownloaders(h);
//        s.AddEventLogDecoders();
//        s.AddEventLogImporters(h);

//        //s.Configure<DeviceEventLoggingConfiguration>(h.Configuration.GetSection(nameof(DeviceEventLoggingConfiguration)));
//    });
//},
//h =>
//{
//    var cmd = h.GetInvocationContext().ParseResult.CommandResult.Command;

//    h.ConfigureServices((h, s) =>
//    {
//        if (cmd is ICommandOption opt)
//        {
//            opt.BindCommandOptions(h, s);
//        }
//    });
//});

//var cmdParser = cmdBuilder.Build();
//await cmdParser.InvokeAsync(args);

//http://10.10.10.73:8080/api/v1/cameras/1/detections?start-time=2024-12-03T23:45:19&end-time=2024-12-04T00:00:00 - This is each event that gets triggered in a certain time period.
//http://10.10.10.73:8080/api/v1/cameras/1/bin-statistics?start-time=2024-12-03T23:43:09&end-time=2024-12-10T00:00:00&intervals=10 - This will pull out the bin stats so it will be events grouped by the interval time


var device = new Device() 
{ 
    DeviceIdentifier = "7114", 
    Ipaddress = "10.210.14.15",
    Location = new Location() { LocationIdentifier = "7114"}
};
device.DeviceConfiguration = new()
{
    Id = 21,
    Firmware = "test",
    ConnectionTimeout = 5000,
    OperationTimeout = 5000,
    Protocol = Utah.Udot.Atspm.Data.Enums.TransportProtocols.Http,
    //Directory = "api/v1/cameras/[Device:DeviceIdentifier]",
    Directory = "v1/asclog/xml/full",
    LoggingOffset = 8,
    SearchTerms = new string[] 
    { 
        "?since=[DateTime:MM-dd-yyyy HH:mm:ss.f]",
        "?since=[DateTime:MM-dd-yyyy HH:mm:ss.f]",
        //"/detections?start-time=[DateTime:yyyy-MM-ddTHH:mm:ss]&end-time=[DateTime:yyyy-MM-ddTHH:mm:ss]",
        //"/bin-statistics?start-time=[DateTime:yyyy-MM-ddTHH:mm:ss]&end-time=[DateTime:yyyy-MM-ddTHH:mm:ss]&intervals=10"
    },
    Port = 80,
    UserName = "user",
    Password = "password",
    ConnectionProperties = new Dictionary<string, string>()
    {
        {"Accept", "application/xml" }
        //{new KeyValuePair<string, string>("Accept", "application/json") },
    },
};


IDownloaderClient client = new HttpDownloaderClient();

var connection = new IPEndPoint(IPAddress.Parse(device.Ipaddress), device.DeviceConfiguration.Port);
var credentials = new NetworkCredential(device.DeviceConfiguration.UserName, device.DeviceConfiguration.Password, device.Ipaddress);

await client.ConnectAsync(connection, credentials, device.DeviceConfiguration.ConnectionTimeout, device.DeviceConfiguration.OperationTimeout, device.DeviceConfiguration.ConnectionProperties);

var searchTerms = device.DeviceConfiguration.SearchTerms.Select(s => new StringObjectParser(device, s).ToString()).ToArray();

var resources = await client.ListDirectoryAsync(new StringObjectParser(device, device.DeviceConfiguration.Directory).ToString(), default, searchTerms);


foreach (var r in resources)
{
    Console.WriteLine($"r: {r}");

    var result = await client.DownloadFileAsync($"C:\\Temp5\\{r.GetHashCode()}.txt", r);


    IEventLogDecoder<IndianaEvent> decoder = new MaxtimeToIndianaDecoder();

    var memoryStream = result.ToMemoryStream();

    memoryStream = decoder.IsCompressed(memoryStream) ? (MemoryStream)decoder.Decompress(memoryStream) : memoryStream;

    var results = decoder.Decode(device, memoryStream);

    Console.WriteLine($"eventCount: {results.Count()}");
}

Console.ReadLine();




