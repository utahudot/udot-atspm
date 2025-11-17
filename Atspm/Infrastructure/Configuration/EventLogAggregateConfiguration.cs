#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/EventLogAggregateConfiguration.cs
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

using System.Text;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    public class EventLogAggregateConfiguration
    {
        public string AggregationType { get; set; }

        public IEnumerable<DateTime> Dates { get; set; }

        /// <summary>
        /// Amount of processes that can be run in parallel
        /// </summary>
        public int ParallelProcesses { get; set; } = 1;

        /// <inheritdoc cref="EventAggregationQueryOptions"/>
        public EventAggregationQueryOptions EventAggregationQueryOptions { get; set; } = new();

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(EventLogAggregateConfiguration)}***************************************************");

            sb.AppendLine($"{nameof(AggregationType)}: {AggregationType}");

            foreach (var i in Dates)
            {
                sb.AppendLine($"{nameof(Dates)}: {i}");
            }

            sb.AppendLine($"{nameof(ParallelProcesses)}: {ParallelProcesses}");

            sb.AppendLine($"{nameof(EventLogAggregateConfiguration)}***************************************************");

            return sb.ToString();
        }
    }
}
