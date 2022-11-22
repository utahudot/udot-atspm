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
    public static class HostingExtensions
    {
        public static void PrintHostInformation(this IServiceProvider services)
        {
            PrintEnvironment(services.GetService<IHostEnvironment>());

            PrintConfiguration(services.GetService<IConfiguration>());
        }

        public static void PrintEnvironment(this IHostEnvironment environment)
        {
            Console.WriteLine($"Application: {environment.ApplicationName}");
            Console.WriteLine($"Environment: {environment.EnvironmentName}");
            Console.WriteLine($"Root Path: {environment.ContentRootPath}");
        }

        public static void PrintConfiguration(this IConfiguration config)
        {
            foreach (var c in config.AsEnumerable())
            {
                Console.WriteLine($"Configuration: {c.Key} - {c.Value}");
            }
        }
    }
}
