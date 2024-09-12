#region license
// Copyright 2024 Utah Department of Transportation
// for DatabaseInstaller - DatabaseInstaller/Program.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using DatabaseInstaller.Commands; // Assuming UpdateCommand and others are in this namespace
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Infrastructure.Extensions;

var rootCmd = new DatabaseInstallerCommands();  // Root command registration
var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();

cmdBuilder.UseHost(a =>
{
    return Host.CreateDefaultBuilder(a)
    .UseConsoleLifetime()  // Ensures the app runs until the console window is closed
    .ConfigureLogging((context, logging) =>
    {
        if (OperatingSystem.IsWindows())
        {
            // Add Event Log or other Windows-specific logging
            // logging.AddEventLog(...);
        }
        // Optionally configure other loggers
        // logging.AddGoogle(...);
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Add required services and repositories for DatabaseInstaller
        services.AddAtspmDbContext(hostContext);
        services.AddAtspmEFConfigRepositories();
        services.AddAtspmEFEventLogRepositories();
        services.AddAtspmEFAggregationRepositories();

        // Register custom commands
        services.AddTransient<UpdateCommand>();  // Add your UpdateCommand
        services.AddSingleton<DatabaseInstallerCommands>();  // Add root command

        services.AddOptions<UpdateCommandConfiguration>()
                .BindCommandLine();
    });
}, host =>
{
    var command = host.GetInvocationContext().ParseResult.CommandResult.Command;

    host.ConfigureServices((context, services) =>
    {
        if (command is ICommandOption option)
        {
            option.BindCommandOptions(context, services);  // Bind command-specific options if needed
        }
    });
});

// Build the command parser and execute
var cmdParser = cmdBuilder.Build();
await cmdParser.InvokeAsync(args);
