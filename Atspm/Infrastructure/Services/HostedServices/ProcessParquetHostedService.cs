using Castle.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.ATSPM.Infrastructure.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    /// <summary>
    /// Background service that discovers .parquet files in the directory, extracts signal IDs
    /// from filenames. Looks up device in the database, and sends valid files through
    /// the DecodeEventLogWorkflow for processing.
    /// </summary>
    public class ProcessParquetHostedService(ILogger<ProcessParquetHostedService> log, IServiceScopeFactory serviceProvider, IOptions<ProcessParquetConfiguration> options) : HostedServiceBase(log, serviceProvider)
    {
        private readonly IOptions<ProcessParquetConfiguration> _options = options;

        /// <inheritdoc/>
        public override async Task Process(IServiceScope scope, Stopwatch stopwatch, CancellationToken cancellationToken = default)
        {
            var repo = scope.ServiceProvider.GetService<IDeviceRepository>();

            var workflow = new DecodeEventLogWorkflow(scope.ServiceProvider.GetService<IServiceScopeFactory>(), 50000, cancellationToken);

            Console.WriteLine($"path: {_options.Value.Path}");

            var dir = new DirectoryInfo(_options.Value.Path);

            if (dir.Exists)
            {
                var files = dir.GetFiles("*.parquet", SearchOption.AllDirectories);

                Console.WriteLine($"file count {files.Length}");

                var groups = files.GroupBy(f => f.Directory.Name);

                foreach (var g in groups)
                {
                    foreach (var file in g)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file.Name);
                        var parts = fileName.Split('_');

                        if (parts.Length > 0)
                        {
                            var signalId = parts[0];
                            var device = repo.GetList().FirstOrDefault(d => d.DeviceIdentifier == signalId);

                            if (device != null)
                            {
                                await workflow.Input.SendAsync(Tuple.Create(device, file));
                            }
                            else
                            {
                                Console.WriteLine($"Device not found for signal ID: {signalId}");
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"directory {_options.Value.Path} doesn't exist");
            }
            workflow.Input.Complete();

            await Task.WhenAll(workflow.Steps.Select(s => s.Completion));
        }
    }
}
