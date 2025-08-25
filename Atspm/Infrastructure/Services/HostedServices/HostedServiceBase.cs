#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.HostedServices/HostedServiceBase.cs
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
using System.Diagnostics;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    /// <summary>
    /// Base class for <see cref="IHostedService"/> with service scope, logging and exit codes
    /// </summary>
    public abstract class HostedServiceBase : IHostedService
    {
        private readonly ILogger _log;
        private readonly IServiceScopeFactory _services;

        /// <summary>
        /// Base class for <see cref="IHostedService"/> with service scope, logging and exit codes
        /// </summary>
        /// <param name="log"></param>
        /// <param name="serviceProvider"></param>
        public HostedServiceBase(ILogger log, IServiceScopeFactory serviceProvider) => (_log, _services) = (log, serviceProvider);

        /// <summary>
        /// The process to execute given the current service scope
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="stopwatch"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Exit Code</returns>
        public abstract Task Process(IServiceScope scope, Stopwatch stopwatch = default, CancellationToken cancellationToken = default);

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
                if (!scope.ServiceProvider.GetService<IHostEnvironment>().IsProduction())
                    scope.ServiceProvider.PrintHostInformation();

                try
                {
                    await Process(scope, sw, cancellationToken);

                    Environment.ExitCode = 0;
                }
                catch (Exception)
                {
                    Environment.ExitCode = 1;

                    throw;
                }
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