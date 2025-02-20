#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.WorkflowSteps/DecodeDeviceData.cs
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
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// Imports and decodes downloaded event logs using the <see cref="IEventLogImporter"/> implementation
    /// </summary>
    public class DecodeDeviceData : TransformManyProcessStepBaseAsync<Tuple<Device, FileInfo>, Tuple<Device, EventLogModelBase>>
    {
        private readonly IServiceScopeFactory _services;

        /// <summary>
        /// Imports and decodes downloaded event logs using the <see cref="IEventLogImporter"/> implementation
        /// </summary>
        /// <param name="services"></param>
        /// <param name="dataflowBlockOptions"></param>
        public DecodeDeviceData(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _services = services;
        }

        /// <inheritdoc/>
        protected override IAsyncEnumerable<Tuple<Device, EventLogModelBase>> Process(Tuple<Device, FileInfo> input, CancellationToken cancelToken = default)
        {
            using (var scope = _services.CreateAsyncScope())
            {
                var importer = scope.ServiceProvider.GetService<IEventLogImporter>();

                return importer.Execute(input, cancelToken);
            }
        }
    }
}
