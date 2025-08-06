#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.HostedServices/DecodeEventLogHostedService.cs
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

using Lextm.SharpSnmpLib.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.ATSPM.Infrastructure.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    public class DecodeEventLogHostedService(ILogger<DecodeEventLogHostedService> log, IServiceScopeFactory serviceProvider, IOptions<DecodeEventsConfiguration> options) : HostedServiceBase(log, serviceProvider)
    {
        private readonly IOptions<DecodeEventsConfiguration> _options = options;

        /// <inheritdoc/>
        public override async Task Process(IServiceScope scope, Stopwatch stopwatch, CancellationToken cancellationToken = default)
        {
            var repo = scope.ServiceProvider.GetService<IDeviceRepository>();

            var workflow = new DecodeEventLogWorkflow(scope.ServiceProvider.GetService<IServiceScopeFactory>(), 50000, cancellationToken);

            Console.WriteLine($"path: {_options.Value.Path}");

            var dir = new DirectoryInfo(_options.Value.Path);

            if (dir.Exists)
            {
                var files = dir.GetFiles("", SearchOption.AllDirectories);

                Console.WriteLine($"file count {files.Length}");

                var groups = files.GroupBy(f => f.Directory.Name);

                foreach (var g in groups)
                {
                    if (g.Key.Contains('-'))
                    {
                        var tag = new[] { g.Key }.Select(s => s.Split('-')).Select(parts => new
                        {
                            Id = parts[0],
                            Ip = parts[1]
                        })
                            .FirstOrDefault();

                        var device = repo.GetList().FirstOrDefault(f => f.DeviceIdentifier == tag.Id);

                        if (device != null)
                        {
                            foreach (var f in g)
                            {
                                await workflow.Input.SendAsync(Tuple.Create(device, f));
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