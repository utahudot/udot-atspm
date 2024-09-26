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

using Microsoft.Extensions.Hosting;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Utah.Udot.Atspm.EventLogUtility.Commands;
using Utah.Udot.Atspm.Infrastructure.Extensions;

var rootCmd = new EventLogCommands();
var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();

cmdBuilder.UseHost(a =>
{
    return Host.CreateDefaultBuilder(a)
    .UseConsoleLifetime()
    .ConfigureLogging((h, l) =>
    {
    })
    .ConfigureServices((h, s) =>
    {
        s.AddAtspmDbContext(h);
        s.AddAtspmEFConfigRepositories();
        s.AddAtspmEFEventLogRepositories();
        s.AddAtspmEFAggregationRepositories();
        s.AddDownloaderClients();
        s.AddDeviceDownloaders(h);
        s.AddEventLogDecoders();
        s.AddEventLogImporters(h);
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