#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.WorkflowSteps/DownloadDeviceData.cs
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
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// Connects to <see cref="Device"/> and downloads event logs using the applicable <see cref="IDeviceDownloader"/>
    /// </summary>
    public class DownloadDeviceData : TransformManyProcessStepBaseAsync<Device, Tuple<Device, FileInfo>>
    {
        private readonly IServiceScopeFactory _services;

        /// <summary>
        /// Connects to <see cref="Device"/> and downloads event logs using the applicable <see cref="IDeviceDownloader"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="dataflowBlockOptions"></param>
        public DownloadDeviceData(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _services = services;
        }

        /// <inheritdoc/>
        protected override IAsyncEnumerable<Tuple<Device, FileInfo>> Process(Device input, CancellationToken cancelToken = default)
        {
            using (var scope = _services.CreateAsyncScope())
            {
                var downloader = scope.ServiceProvider.GetServices<IDeviceDownloader>().First(c => c.CanExecute(input));

                return downloader.Execute(input, cancelToken);
            }
        }
    }
}
