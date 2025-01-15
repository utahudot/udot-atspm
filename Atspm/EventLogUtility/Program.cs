//#region license
//// Copyright 2024 Utah Departement of Transportation
//// for EventLogUtility - %Namespace%/Program.cs
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
//// http://www.apache.org/licenses/LICENSE-2.
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.
//#endregion

//using Microsoft.Extensions.Hosting;
//using System.CommandLine.Builder;
//using System.CommandLine.Hosting;
//using System.CommandLine.Parsing;
//using Utah.Udot.Atspm.EventLogUtility.Commands;
//using Utah.Udot.Atspm.Infrastructure.Extensions;

//var rootCmd = new EventLogCommands();
//var cmdBuilder = new CommandLineBuilder(rootCmd);
//cmdBuilder.UseDefaults();

//cmdBuilder.UseHost(a =>
//{
//    return Host.CreateDefaultBuilder(a)
//    .UseConsoleLifetime()
//    .ConfigureLogging((h, l) =>
//    {
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.ATSPM.Infrastructure.Workflows;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((h, c) =>
{
    c.AddUserSecrets<Program>(optional: true);
})
    .ConfigureServices((h, s) =>
    {
        //s.AddDownloaderClients();
        //s.AddDeviceDownloaders(h);
        //s.AddEventLogDecoders();
        //s.AddEventLogImporters(h);
        //s.AddAtspmEFEventLogRepositories();

        s.AddAtspmDbContext(h);
        s.AddAtspmEFConfigRepositories();
        s.AddAtspmEFEventLogRepositories();
        s.AddAtspmEFAggregationRepositories();
        s.AddDownloaderClients();
        s.AddDeviceDownloaders(h);
        s.AddEventLogDecoders();
        s.AddEventLogImporters(h);
    }).Build();

host.Services.PrintHostInformation();

using (var scope = host.Services.CreateScope())
{
    var newDeviceConfiguration = new DeviceConfiguration
    {
        Id = 12345,
        Firmware = "Vision Camera", // Placeholder for firmware version
        Notes = string.Empty, // Placeholder for configuration notes
        Protocol = TransportProtocols.Http, // Adjust based on your enum values
        Port = 8080, // Placeholder for the logging communication port
        Directory = "api/v1/cameras/1/detections", // Placeholder for the path to log directory STATS - api/v1/cameras/1/bin-statistics DET - api/v1/cameras/1/detections
        SearchTerms = Array.Empty<string>(), // Initialize as an empty array
        ConnectionTimeout = 2000, // Placeholder for device connection timeout
        OperationTimeout = 5000, // Placeholder for device operation timeout
        Decoders = new[] { "JsonToVisionCameraDetectorEventDecoder" }, // Initialize as an empty array STATS - JsonToVisionCameraStatisticEventDecoder DET - JsonToVisionCameraDetectorEventDecoder, 
        //UserName = string.Empty, // Placeholder for username
        //Password = string.Empty, // Placeholder for password
        //ProductId = null, // Nullable Product ID
        //Product = null, // No initial related product
        //Devices = new HashSet<Device>() // Initialize with an empty collection
    };

    var newLocation = new Location
    {
        Latitude = 0.0,
        Longitude = 0.0,
        PrimaryName = "hey",
        SecondaryName = "other",
        ChartEnabled = false,
        VersionAction = LocationVersionActions.Initial, // Adjust based on your enum values
        Note = string.Empty,
        Start = DateTime.MinValue,
        PedsAre1to1 = false,
        LocationIdentifier = "123",
        JurisdictionId = null,
        Jurisdiction = null, // Assuming no data for this related entity initially
        LocationTypeId = 0,
        LocationType = null, // Assuming no data for this related entity initially
        RegionId = null,
        Region = null, // Assuming no data for this related entity initially
        //Approaches = new HashSet<Approach>(), // Initialize as an empty collection
        //Areas = new HashSet<Area>(), // Initialize as an empty collection
        //Devices = new HashSet<Device> { newDevice } // Initialize as an empty collection
    };

    var newDevice = new Device
    {
        Id = 67890,
        LoggingEnabled = true,
        Ipaddress = "10.10.10.73", // Placeholder for the device's IP address
        DeviceStatus = DeviceStatus.Active, // Adjust based on your enum values
        DeviceType = DeviceTypes.AICamera, // Adjust based on your enum values
        Notes = string.Empty, // Placeholder for device notes
        LocationId = 1000, // Placeholder ID; update with a valid location ID if available
        Location = newLocation, // No related location data initially
        DeviceConfigurationId = 12345, // Optional configuration ID
        DeviceConfiguration = newDeviceConfiguration // No related configuration data initially
    };


    //var downloader = scope.ServiceProvider.GetService<IDeviceDownloader>();
    //var result = downloader.Execute(newDevice);
    //await foreach (var r in result)
    //{
    //    Console.WriteLine(r.Item2.FullName);
    //}

    //var decoders = scope.ServiceProvider.GetServices<IEventLogDecoder>();
    //foreach (var r in decoders)
    //{
    //    Console.WriteLine(r.GetType().FullName);
    //}

    //var file = new FileInfo("C:\\temp\\This Is The Camera TesT - \\AICamera\\10.10.10.73\\638701216240621911.json");

    //var fileImporter = scope.ServiceProvider.GetService<IEventLogImporter>();
    //var result = fileImporter.Execute(Tuple.Create(newDevice, file));
    //await foreach (var r in result)
    //{
    //    Console.WriteLine(r.Item2.ToString());
    //}

    var workflow = new DeviceEventLogWorkflow(scope.ServiceProvider.GetService<IServiceScopeFactory>(), 50000, 1);

    await Task.Delay(TimeSpan.FromSeconds(2));
    await workflow.Input.SendAsync(newDevice);
    workflow.Input.Complete();

    await Task.WhenAll(workflow.Steps.Select(s => s.Completion));

    //var eventLogRepository = scope.ServiceProvider.GetService<IEventLogRepository>();
    //var statEventLogRepository = scope.ServiceProvider.GetService<IVisionCameraStatisticsEventLogRepository>();
    //var detEventLogRepository = scope.ServiceProvider.GetService<IVisionCameraDetectionEventLogRepository>();

    //var start = Convert.ToDateTime("2020-01-01");
    //var end = Convert.ToDateTime("2026-01-01");

    //var statEvent = statEventLogRepository.GetEventsBetweenDates(newLocation.LocationIdentifier, start, end);
    //var detEvent = detEventLogRepository.GetEventsBetweenDates(newLocation.LocationIdentifier, start, end);

    var integer = 2;
}
Console.ReadLine();