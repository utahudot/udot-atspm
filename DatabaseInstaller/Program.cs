﻿#region license
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

using DatabaseInstaller.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Infrastructure.Extensions;

var rootCmd = new DatabaseInstallerCommands();
var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();

cmdBuilder.UseHost(hostBuilder =>
{
    return Host.CreateDefaultBuilder(hostBuilder)
    .UseConsoleLifetime()
    .ConfigureAppConfiguration((h, c) =>
    {
        c.AddCommandLine(args);
        c.AddUserSecrets<Program>(optional: true);

    })
    .ConfigureLogging((hostContext, logging) =>
    {
        // Configure logging if needed
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Core services
        services.AddAtspmDbContext(hostContext);
        services.AddAtspmEFConfigRepositories();
        services.AddAtspmEFEventLogRepositories();

        // Optional: Register any core services your application might need here.
    });
},
host =>
{
    var cmd = host.GetInvocationContext().ParseResult.CommandResult.Command;

    // Dynamically bind services for the specific command being executed
    host.ConfigureServices((context, services) =>
    {
        if (cmd is ICommandOption commandOption)
        {
            // Call the BindCommandOptions method for the command
            commandOption.BindCommandOptions(context, services);
        }
    });
});

// Build and invoke the command parser
var cmdParser = cmdBuilder.Build();
var parseResult = cmdParser.Parse(args);
Console.WriteLine("Command: " + parseResult.CommandResult.Command.Name);
await cmdParser.InvokeAsync(args);
