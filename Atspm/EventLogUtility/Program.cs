#region license
// Copyright 2025 Utah Departement of Transportation
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

using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;
using Utah.Udot.Atspm.EventLogUtility.Commands;
using Utah.Udot.Atspm.Infrastructure.Extensions;

//tricking github 11111

if (OperatingSystem.IsWindows())
{
    if (!EventLog.SourceExists("Atspm"))
        EventLog.CreateEventSource(AppDomain.CurrentDomain.FriendlyName, "Atspm");
}

var rootCmd = new EventLogCommands();
var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();
cmdBuilder.UseHost(a =>
{
    return Host.CreateDefaultBuilder(a)
    //.UseConsoleLifetime()
    .ApplyVolumeConfiguration()
    .ConfigureLogging((h, l) =>
    {
        if (OperatingSystem.IsWindows())
        {
            l.AddEventLog(c =>
            {
                c.SourceName = AppDomain.CurrentDomain.FriendlyName;
                c.LogName = "Atspm";
            });
        }

        //l.AddGoogle(new LoggingServiceOptions
        //{
        //    ServiceName = AppDomain.CurrentDomain.FriendlyName,
        //    Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
        //    Options = LoggingOptions.Create(LogLevel.Debug, AppDomain.CurrentDomain.FriendlyName)
        //});
    })
    .ConfigureServices((h, s) =>
    {
        //s.AddGoogleDiagnostics(loggingOptions: LoggingOptions.Create(LogLevel.Debug));

        s.AddAtspmDbContext(h);
        s.AddAtspmEFConfigRepositories();
        s.AddAtspmEFEventLogRepositories();
        s.AddAtspmEFAggregationRepositories();
        s.AddDownloaderClients();
        s.AddDeviceDownloaders(h);
        s.AddEventLogDecoders();
        s.AddEventLogImporters(h);

        //s.Configure<DeviceEventLoggingConfiguration>(h.Configuration.GetSection(nameof(DeviceEventLoggingConfiguration)));
    });
},
h =>
{
    var cmd = h.GetInvocationContext().ParseResult.CommandResult.Command;

    h.ConfigureServices((h, s) =>
    {
        if (cmd is ICommandOption opt)
        {
            opt.BindCommandOptions(h, s);
        }
    });
});

var cmdParser = cmdBuilder.Build();
await cmdParser.InvokeAsync(args);

//var host = Host.CreateDefaultBuilder(args)
//    .ConfigureLogging((h, l) =>
//    {
//        l.AddGoogle(new LoggingServiceOptions
//        {
//            //ProjectId = "1022556126938",
//            ServiceName = AppDomain.CurrentDomain.FriendlyName,
//            Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
//            Options = LoggingOptions.Create(LogLevel.Debug, AppDomain.CurrentDomain.FriendlyName)
//        });
//    }).Build();

//using (var scope = host.Services.CreateScope())
//{
//    var logger = scope.ServiceProvider.GetService<ILogger<Program>>()
//    .WithAddedLabels(new Dictionary<string, string>()
//{
//    {"key1", "value1"},
//    {"key2", "value2"},
//    {"key3", "value3"},
//    {"key4", "value4"},
//    {"key5", "value5"},
//});

//    logger.LogDebug("log debug test");
//    logger.LogInformation("log info test");
//    logger.LogWarning("log warning test");
//    logger.LogError("log error test");
//    logger.LogError(new Exception("exception test"), "log error with exception test");

//    await Task.Delay(TimeSpan.FromSeconds(10));
//}
