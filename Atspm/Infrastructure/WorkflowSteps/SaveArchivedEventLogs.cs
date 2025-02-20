#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.WorkflowSteps/SaveArchivedEventLogs.cs
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
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// Persists <see cref="CompressedEventLogBase"/> entries
    /// </summary>
    public class SaveArchivedEventLogs : TransformManyProcessStepBaseAsync<CompressedEventLogBase, CompressedEventLogBase>
    {
        private readonly IServiceScopeFactory _services;

        /// <summary>
        /// Persists <see cref="CompressedEventLogBase"/> entries
        /// </summary>
        /// <param name="services"></param>
        /// <param name="dataflowBlockOptions"></param>
        public SaveArchivedEventLogs(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _services = services;
        }

        /// <inheritdoc/>
        protected override async IAsyncEnumerable<CompressedEventLogBase> Process(CompressedEventLogBase input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            using (var scope = _services.CreateAsyncScope())
            {
                var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

                yield return await repo.Upsert(input);
            }
        }
    }
}
