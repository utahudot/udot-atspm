#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Extensions/IndianaEventLogRepositoryExtensions.cs
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

using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IIndianaEventLogRepository"/>
    /// </summary>
    public static class IndianaEventLogRepositoryExtensions
    {
        /// <summary>
        /// Gets <see cref="IIndianaEventLogRepository"/> events by <paramref name="eventCodes"/> and <paramref name="param"/>
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="locationId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="eventCodes"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodesParam(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<short> eventCodes, int param)
        {
            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .FromSpecification(new IndianaLogCodeAndParamSpecification(eventCodes, param))
                .ToList();

            return result;
        }

        /// <summary>
        /// Gets <see cref="IIndianaEventLogRepository"/> events by <paramref name="eventCodes"/> and <paramref name="param"/>
        /// then applies <paramref name="latencyCorrection"/> to the <see cref="ITimestamp.Timestamp"/>
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="locationId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="eventCodes"></param>
        /// <param name="param"></param>
        /// <param name="latencyCorrection"></param>
        /// <returns></returns>
        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodesParam(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<short> eventCodes, int param, double latencyCorrection)
        {
            var result = repo.GetEventsByEventCodesParam(locationId, startTime, endTime, eventCodes, param);

            foreach (var item in result)
            {
                item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
            }

            return result;
        }

        /// <summary>
        /// Gets <see cref="IIndianaEventLogRepository"/> events by <paramref name="eventCodes"/> and <paramref name="param"/>
        /// then applies <paramref name="latencyCorrection"/> and <paramref name="offset"/> to the <see cref="ITimestamp.Timestamp"/>
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="locationId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="eventCodes"></param>
        /// <param name="param"></param>
        /// <param name="offset"></param>
        /// <param name="latencyCorrection"></param>
        /// <returns></returns>
        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodesParam(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<short> eventCodes, int param, double offset, double latencyCorrection)
        {
            var result = repo.GetEventsByEventCodesParam(locationId, startTime, endTime, eventCodes, param);

            foreach (var item in result)
            {
                item.Timestamp = item.Timestamp.AddMilliseconds(offset);
                item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
            }

            return result;
        }

        /// <summary>
        /// Gets <see cref="IIndianaEventLogRepository"/> events by <paramref name="eventCodes"/> and <paramref name="eventParameters"/>
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="locationId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="eventParameters"></param>
        /// <param name="eventCodes"></param>
        /// <returns></returns>
        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodesParam(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<short> eventCodes)
        {
            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .FromSpecification(new IndianaLogCodeAndParamSpecification(eventCodes, eventParameters))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        /// <summary>
        /// Gets the count of events by <paramref name="eventCodes"/> and <paramref name="eventParameters"/>
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="locationId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="eventParameters"></param>
        /// <param name="eventCodes"></param>
        /// <returns></returns>
        public static int GetEventCountByCodesParam(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<short> eventCodes)
        {
            return repo.GetEventsByEventCodesParam(locationId, startTime, endTime, eventParameters, eventCodes).Count;
        }

        //public static IReadOnlyList<IndianaEvent> GetAllAggregationCodes(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime)
        //{
        //    var codes = new List<short>
        //    {
        //        150,
        //        114,
        //        113,
        //        112,
        //        105,
        //        102,
        //        1
        //    };

        //    var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
        //        .FromSpecification(new IndianaLogCodeAndParamSpecification(codes))
        //        .ToList();

        //    return result;
        //}

        //public static int GetDetectorActivationCount(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, int detectorChannel)
        //{
        //    var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
        //        .FromSpecification(new IndianaLogCodeAndParamSpecification(82, detectorChannel))
        //        .ToList().Count;

        //    return result;
        //}

        /// <summary>
        /// Gets <see cref="IIndianaEventLogRepository"/> events by <paramref name="eventCode"/>
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="locationId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="eventCode"></param>
        /// <returns></returns>
        public static IReadOnlyList<IndianaEvent> GetEventsByEventCode(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, short eventCode)
        {
            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .FromSpecification(new IndianaLogCodeAndParamSpecification(eventCode))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        /// <summary>
        /// Gets <see cref="IIndianaEventLogRepository"/> events by <paramref name="eventCodes"/>
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="locationId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="eventCodes"></param>
        /// <returns></returns>
        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodes(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<short> eventCodes)
        {
            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .FromSpecification(new IndianaLogCodeAndParamSpecification(eventCodes))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        //public static IReadOnlyList<IndianaEvent> GetSplitEvents(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime)
        //{
        //    var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
        //        .Where(i => (int)i.EventCode > 130 && (int)i.EventCode < 150)
        //        .OrderBy(o => o.EventParam)
        //        .ToList();

        //    return result;
        //}
    }
}
