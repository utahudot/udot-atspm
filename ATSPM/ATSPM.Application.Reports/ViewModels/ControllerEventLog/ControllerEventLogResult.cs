using ATSPM.Data.Models;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.ControllerEventLog
{
    public class ControllerEventLogResult
    {
        public string SignalId { get; }
        public DateTime StartDate { get; protected set; }
        public DateTime EndDate { get; protected set; }
        public List<int> EventCodes { get; }
        public List<Data.Models.ControllerEventLog> Events { get; set; }


        public ControllerEventLogResult(string signalId, DateTime startDate, DateTime endDate, List<int> eventCodes, List<Data.Models.ControllerEventLog> events)
        {
            SignalId = signalId;
            StartDate = startDate;
            EndDate = endDate;
            EventCodes = eventCodes;
            Events = events;
        }

        public ControllerEventLogResult()
        {
        }
    }
}
