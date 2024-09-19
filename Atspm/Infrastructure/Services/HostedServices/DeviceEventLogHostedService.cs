#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.HostedServices/SignalLoggerUtilityHostedService.cs
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.ATSPM.Infrastructure.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    /// <summary>
    /// Hosted service for running the <see cref="DeviceEventLogWorkflow"/>
    /// </summary>
    public class DeviceEventLogHostedService : IHostedService
    {
        private readonly ILogger _log;
        private readonly IServiceScopeFactory _services;
        private readonly IOptions<DeviceEventLoggingConfiguration> _options;

        /// <summary>
        /// Hosted service for running the <see cref="DeviceEventLogWorkflow"/>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        public DeviceEventLogHostedService(ILogger<DeviceEventLogHostedService> log, IServiceScopeFactory serviceProvider, IOptions<DeviceEventLoggingConfiguration> options) =>
            (_log, _services, _options) = (log, serviceProvider, options);

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var serviceName = this.GetType().Name;
            var logMessages = new HostedServiceLogMessages(_log, this.GetType().Name);

            cancellationToken.Register(() => logMessages.StartingCancelled(serviceName));
            logMessages.StartingService(serviceName);

            var sw = new Stopwatch();
            sw.Start();

            using (var scope = _services.CreateAsyncScope())
            {
                var workflow = new DeviceEventLogWorkflow(_services, _options.Value.BatchSize, _options.Value.ParallelProcesses, cancellationToken);

                var repo = scope.ServiceProvider.GetService<IDeviceRepository>();

                await foreach (var d in repo.GetDevicesForLogging(_options.Value.DeviceEventLoggingQueryOptions))
                {
                    await workflow.Input.SendAsync(d);
                }

                workflow.Input.Complete();

                await Task.WhenAll(workflow.Steps.Select(s => s.Completion));
            }

            sw.Stop();

            logMessages.CompletingService(serviceName, sw.Elapsed);
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            var serviceName = this.GetType().Name;
            var logMessages = new HostedServiceLogMessages(_log, this.GetType().Name);

            cancellationToken.Register(() => logMessages.StoppingCancelled(serviceName));
            logMessages.StoppingService(serviceName);

            return Task.CompletedTask;
        }
    }
}