#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Repositories.EventLogRepositories/IEventLogRepository.cs
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

using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.AggregationRepositories;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.EventLogRepositories
{
    /// <summary>
    /// Device event log repository
    /// </summary>
    public interface IEventLogRepository : ICompressedDataRepository<CompressedEventLogBase>
    {
        ///// <summary>
        ///// Get archived events that match <paramref name="locationIdentifier"/> and <paramref name="start"/>/<paramref name="end"/>
        ///// </summary>
        ///// <param name="locationIdentifier">Location identifier</param>
        ///// <param name="start">Archive date of event to start with</param>
        ///// <param name="end">Archive date of event to end with</param>
        ///// <returns></returns>
        //IAsyncEnumerable<CompressedEventLogBase> GetData(string locationIdentifier, DateTime start, DateTime end);

        ///// <summary>
        ///// Get archived events that match <paramref name="locationIdentifier"/>, <paramref name="start"/>/<paramref name="end"/> and <paramref name="dataType"/>
        ///// </summary>
        ///// <param name="locationIdentifier">Location identifier</param>
        ///// <param name="start">Archive date of event to start with</param>
        ///// <param name="end">Archive date of event to end with</param>
        ///// <param name="dataType">Type that inherits from <see cref="EventLogModelBase"/></param>
        ///// <returns></returns>
        //IAsyncEnumerable<CompressedEventLogBase> GetData(string locationIdentifier, DateTime start, DateTime end, Type dataType);

        /// <summary>
        /// Get archived events that match <paramref name="locationIdentifier"/>, <paramref name="start"/>/<paramref name="end"/> and <paramref name="deviceId"/>
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="start">Archive date of event to start with</param>
        /// <param name="end">Archive date of event to end with</param>
        /// <param name="dataType">Type that inherits from <see cref="EventLogModelBase"/></param>
        /// <param name="deviceId">Deivce id events came from</param>
        /// <returns></returns>
        IAsyncEnumerable<CompressedEventLogBase> GetData(string locationIdentifier, DateTime start, DateTime end, Type dataType, int deviceId);

        /// <summary>
        /// Get archived event that matches <paramref name="locationIdentifier"/>, <paramref name="start"/>/<paramref name="end"/> and <paramref name="deviceId"/>
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="start">Archive date of event to start with</param>
        /// <param name="end">Archive date of event to end with</param>
        /// <param name="deviceId">Deivce id events came from</param>
        /// <returns></returns>
        IAsyncEnumerable<CompressedEventLogBase> GetData(string locationIdentifier, DateTime start, DateTime end, int deviceId);

        /// <summary>
        /// Get archived events that match <paramref name="locationIdentifier"/> and <paramref name="start"/>/<paramref name="end"/>
        /// Where date type of derived from <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Data type of <see cref="EventLogModelBase"/></typeparam>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="start">Archive date of event to start with</param>
        /// <param name="end">Archive date of event to end with</param>
        /// <returns></returns>
        IAsyncEnumerable<CompressedEventLogs<T>> GetData<T>(string locationIdentifier, DateTime start, DateTime end) where T : EventLogModelBase;

        /// <summary>
        /// Get archived events that match <paramref name="locationIdentifier"/>, <paramref name="start"/>/<paramref name="end"/> and <paramref name="deviceId"/>
        /// Where date type is derived from <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Data type of <see cref="EventLogModelBase"/></typeparam>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="start">Archive date of event to start with</param>
        /// <param name="end">Archive date of event to end with</param>
        /// <param name="deviceId">Deivce id events came from</param>
        /// <returns></returns>
        IAsyncEnumerable<CompressedEventLogs<T>> GetData<T>(string locationIdentifier, DateTime start, DateTime end, int deviceId) where T : EventLogModelBase;
    }

    /// <summary>
    /// Generic interface for accessing device event logs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEventLogRepository<T> : IAsyncRepository<CompressedEventLogs<T>> where T : EventLogModelBase
    {
        /// <summary>
        /// Get all <see cref="EventLogModelBase"/> logs by <see cref="EventLogModelBase.LocationIdentifier"/> and date range
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="start">Start time</param>
        /// <param name="end">End time</param>
        /// <returns></returns>
        IReadOnlyList<T> GetEventsBetweenDates(string locationIdentifier, DateTime start, DateTime end);
    }
}