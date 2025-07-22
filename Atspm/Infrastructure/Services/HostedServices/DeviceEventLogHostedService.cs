#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.HostedServices/DeviceEventLogHostedService.cs
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
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.ATSPM.Infrastructure.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    /// <summary>
    /// Hosted service for running the <see cref="DeviceEventLogWorkflow"/>
    /// </summary>
    /// <remarks>
    /// Hosted service for running the <see cref="DeviceEventLogWorkflow"/>
    /// </remarks>
    /// <param name="log"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="options"></param>
    public class DeviceEventLogHostedService(ILogger<DeviceEventLogHostedService> log, IServiceScopeFactory serviceProvider, IOptions<DeviceEventLoggingConfiguration> options) : HostedServiceBase(log, serviceProvider)
    {
        private readonly IOptions<DeviceEventLoggingConfiguration> _options = options;

        /// <inheritdoc/>
        public override async Task Process(IServiceScope scope, Stopwatch stopwatch, CancellationToken cancellationToken = default)
        {
            var repo = scope.ServiceProvider.GetService<IDeviceRepository>();

            var workflow = new DeviceEventLogWorkflow(scope.ServiceProvider.GetService<IServiceScopeFactory>(), _options.Value.BatchSize, _options.Value.ParallelProcesses, cancellationToken);

            await foreach (var d in repo.GetDevicesForLogging(_options.Value.DeviceEventLoggingQueryOptions))
            {
                await workflow.Input.SendAsync(d);
            }

            workflow.Input.Complete();

            await Task.WhenAll(workflow.Steps.Select(s => s.Completion));
        }
    }
}