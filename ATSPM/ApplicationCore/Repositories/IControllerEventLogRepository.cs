#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Repositories/IControllerEventLogRepository.cs
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
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Location Controller Event Log Repository
    /// </summary>
    public interface IControllerEventLogRepository : IAsyncRepository<ControllerLogArchive>
    {
        /// <summary>
        /// Get all controller event logs by <c>LocationId</c> and date range
        /// </summary>
        /// <param name="locationId">Location controller identifier</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns></returns>
        IReadOnlyList<ControllerEventLog> GetLocationEventsBetweenDates(string locationId, DateTime startTime, DateTime endTime);

        #region ExtensionMethods

        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string LocationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param);

        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string LocationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection);

        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string LocationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection);

        //IReadOnlyList<ControllerEventLog> GetRecordsByParameterAndEvent(string LocationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes);

        //int GetRecordCountByParameterAndEvent(string LocationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes);

        //IReadOnlyList<ControllerEventLog> GetAllAggregationCodes(string LocationId, DateTime startTime, DateTime endTime);

        //int GetApproachEventsCountBetweenDates(int approachId, DateTime startTime, DateTime endTime, int phaseNumber);

        //int GetDetectorActivationCount(string LocationId, DateTime startTime, DateTime endTime, int detectorChannel);

        //ControllerEventLog GetFirstEventBeforeDate(string LocationId, int eventCode, DateTime date);

        //ControllerEventLog GetFirstEventBeforeDateByEventCodeAndParameter(string LocationId, int eventCode, int eventParam, DateTime date);

        //IReadOnlyList<ControllerEventLog> GetLocationEventsByEventCode(string LocationId, DateTime startTime, DateTime endTime, int eventCode);

        //IReadOnlyList<ControllerEventLog> GetLocationEventsByEventCodes(string LocationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes);

        //IReadOnlyList<ControllerEventLog> GetSplitEvents(string LocationId, DateTime startTime, DateTime endTime);

        //double GetTmcVolume(DateTime startDate, DateTime endDate, string LocationId, int phase);

        //IReadOnlyList<ControllerEventLog> GetTopEventsAfterDateByEventCodesParam(string LocationId, DateTime timestamp, IEnumerable<int> eventCodes, int param, int top);

        //IReadOnlyList<ControllerEventLog> GetTopNumberOfLocationEventsBetweenDates(string LocationId, int numberOfRecords, DateTime startTime, DateTime endTime);

        #endregion

        #region Obsolete

        //[Obsolete("Use GetLocationEventsBetweenDates(locationId, startTime, endTime).Count()", true)]
        //int GetLocationEventsCountBetweenDates(string LocationId, DateTime startTime, DateTime endTime);

        //[Obsolete("This method isn't currently being used")]
        //bool CheckForRecords(string LocationId, DateTime startTime, DateTime endTime);

        //[Obsolete("This method isn't currently being used")]
        //int GetEventCountByEventCodesParamDateTimeRange(string LocationId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param);

        //[Obsolete("This method isn't currently being used")]
        //IReadOnlyList<ControllerEventLog> GetEventsBetweenDates(DateTime startTime, DateTime endTime);

        //[Obsolete("This method isn't currently being used")]
        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamDateTimeRange(string LocationId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param);

        //[Obsolete("Use GetEventsByEventCodesParam overload", true)]
        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithLatencyCorrection(string LocationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection);

        //[Obsolete("Use GetEventsByEventCodesParam overload", true)]
        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(string LocationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection);

        //[Obsolete("Use GetLocationEventsCountBetweenDates instead", true)]
        //int GetRecordCount(string LocationId, DateTime startTime, DateTime endTime);

        //[Obsolete("Depreciated, use a specification instead")]
        //DateTime GetMostRecentRecordTimestamp(string LocationId);

        #endregion
    }
}
