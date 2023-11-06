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
