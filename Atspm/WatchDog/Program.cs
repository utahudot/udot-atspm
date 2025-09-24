#region license
// Copyright 2025 Utah Departement of Transportation
// for WatchDog - %Namespace%/Program.cs
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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.Atspm.Services;
using Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices;
using Utah.Udot.ATSPM.WatchDog.Commands;

//gitactions: IIII

var rootCmd = new WatchdogCommand();
var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();

cmdBuilder.UseHost(hostBuilder =>
{
    return Host.CreateDefaultBuilder(args)
    //.UseConsoleLifetime()
    .ApplyVolumeConfiguration()
    .ConfigureLogging((h, l) =>
    {
        l.AddGoogle(h);
    })
    .ConfigureServices((h, s) =>
    {
        s.AddEmailServices(h);
        s.AddAtspmDbContext(h);
        s.AddAtspmEFConfigRepositories();
        s.AddAtspmEFEventLogRepositories();
        s.AddAtspmEFAggregationRepositories();
        s.AddScoped<IWatchdogEmailService, WatchdogEmailService>();
        s.AddScoped<WatchDogLogService>();
        s.AddScoped<ScanService>();
        s.AddScoped<PlanService>();
        s.AddScoped<AnalysisPhaseCollectionService>();
        s.AddScoped<AnalysisPhaseService>();
        s.AddScoped<PhaseService>();
        s.AddScoped<SegmentedErrorsService>();
        s.AddScoped<IWatchDogIgnoreEventService, WatchDogIgnoreEventService>();

        // Register the hosted service with the date
        s.AddIdentity<ApplicationUser, IdentityRole>() // Add this line to register Identity
            .AddEntityFrameworkStores<IdentityContext>() // Specify the EF Core store
            .AddDefaultTokenProviders();
    });
},
host =>
{
    var cmd = host.GetInvocationContext().ParseResult.CommandResult.Command;

    host.ConfigureServices((context, services) =>
    {
        if (cmd is ICommandOption commandOption)
        {
            commandOption.BindCommandOptions(context, services);
        }
    });
});

var cmdParser = cmdBuilder.Build();
await cmdParser.InvokeAsync(args);