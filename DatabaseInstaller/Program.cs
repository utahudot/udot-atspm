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

using DatabaseInstaller.Commands;
using DatabaseInstaller.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.Extensions;

var rootCmd = new DatabaseInstallerCommands();  // Root command registration
var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();

cmdBuilder.UseHost(a =>
{
    return Host.CreateDefaultBuilder(args)  // Use 'args' instead of 'hostBuilder.Args'
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

            // Ensure command-line options take precedence over configuration file settings
            services.Configure<UpdateCommandConfiguration>(hostContext.Configuration.GetSection("CommandLineOptions"));
            services.AddOptions<UpdateCommandConfiguration>()
                    .BindCommandLine();  // Bind command-line arguments to UpdateCommandConfiguration

            services.AddIdentity<ApplicationUser, IdentityRole>()  // Ensure that Identity services are registered
                .AddEntityFrameworkStores<IdentityContext>()       // Use the IdentityContext for storing Identity-related data
                .AddDefaultTokenProviders();

            // Register UpdateCommand and its hosted service
            services.AddTransient<UpdateCommand>();
            services.AddScoped<UpdateCommandHostedService>();

            // Add UpdateCommandConfiguration as a singleton service to the service collection
            services.AddSingleton<UpdateCommandConfiguration>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<UpdateCommandConfiguration>>().Value;
                return config;
            });

            // Register the HostedService to be used when UpdateCommand is invoked
            services.AddHostedService<UpdateCommandHostedService>();
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



