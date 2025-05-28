#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.HostedServices/EventAggregationHostedService.cs
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    /// <summary>
    /// Hosted service for transferring event logs between repositories.
    /// </summary>
    public class EventLogTransferHostedService : HostedServiceBase
    {
        private readonly IOptions<EventLogTransferOptions> _options;

        /// <summary>
        /// Hosted service for transferring event logs between repositories.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        public EventLogTransferHostedService(ILogger<EventAggregationHostedService> log, IServiceScopeFactory serviceProvider, IOptions<EventLogTransferOptions> options) : base(log, serviceProvider)
        {
            _options = options;
        }

        public override Task Process(IServiceScope scope, CancellationToken cancellationToken = default)
        {
            Console.WriteLine(_options.Value);

            return Task.CompletedTask;
        }
    }
}