#region license
// Copyright 2026 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.Services/IEventLogImporterService.cs
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

namespace Utah.Udot.Atspm.DataApi.Services
{
    /// <summary>
    /// Converts Indiana events to compressed daily logs and persists them.
    /// </summary>
    public interface IEventLogImporterService
    {
        /// <summary>
        /// Groups events into compressed daily logs for the location's active controller.
        /// </summary>
        IReadOnlyList<CompressedEventLogs<IndianaEvent>> CompressEvents(
            string locationIdentifier,
            IReadOnlyCollection<IndianaEvent> events);

        /// <summary>
        /// Inserts compressed logs, treating an existing log as a successful import.
        /// </summary>
        Task<bool> InsertLogsWithRetryAsync(
            IReadOnlyCollection<CompressedEventLogs<IndianaEvent>> archiveLogs,
            CancellationToken cancelToken = default);
    }
}
