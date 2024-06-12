#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Extensions/HostingExtensions.cs
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Domain.Extensions
{
    /// <summary>
    /// Microsoft Hosting Extension Helpers
    /// </summary>
    public static class HostingExtensions
    {
        /// <summary>
        /// Print hosting information
        /// </summary>
        /// <param name="services"></param>
        public static void PrintHostInformation(this IServiceProvider services)
        {
            PrintEnvironment(services.GetService<IHostEnvironment>());

            PrintConfiguration(services.GetService<IConfiguration>());
        }

        /// <summary>
        /// Print environment information
        /// </summary>
        /// <param name="environment"></param>
        public static void PrintEnvironment(this IHostEnvironment environment)
        {
            Console.WriteLine($"Application: {environment.ApplicationName}");
            Console.WriteLine($"Environment: {environment.EnvironmentName}");
            Console.WriteLine($"Root Path: {environment.ContentRootPath}");
        }

        /// <summary>
        /// Print configuration information
        /// </summary>
        /// <param name="config"></param>
        public static void PrintConfiguration(this IConfiguration config)
        {
            foreach (var c in config.AsEnumerable())
            {
                Console.WriteLine($"Configuration: {c.Key} - {c.Value}");
            }
        }
    }
}
