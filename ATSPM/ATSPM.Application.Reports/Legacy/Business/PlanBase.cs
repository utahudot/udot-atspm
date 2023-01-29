using System;
using System.Collections.Generic;
using ATSPM.Application.Reports.ViewModels.ControllerEventLog;
using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Application.Enums;
using System.Collections.Immutable;
using System.Linq;

namespace Legacy.Common.Business
{
    public class PlansBaseService
    {
        private readonly IControllerEventLogRepository _controllerEventLogRepository;

        public PlansBaseService(IControllerEventLogRepository controllerEventLogRepository)
        {
            _controllerEventLogRepository = controllerEventLogRepository;
        }

        public List<ControllerEventLog> GetEvents(string signalID, DateTime startDate, DateTime endDate)
        {
            var events = _controllerEventLogRepository.GetSignalEventsByEventCode(signalID, startDate, endDate, 131).ToList();

            //Get the plan Previous to the start date
            var tempEvent = new ControllerEventLog();
            tempEvent.SignalId = signalID;
            tempEvent.Timestamp = startDate;
            tempEvent.EventCode = 131;
            tempEvent.EventParam = GetPreviousPlan(signalID, startDate);

            events.Insert(0, tempEvent);

            //Remove Duplicate Plans
            var x = -1;
            var temp = new List<ControllerEventLog>();
            foreach (var cel in events)
                temp.Add(cel);
            foreach (var cel in temp)
                if (x == -1)
                {
                    x = cel.EventParam;
                }
                else if (x != cel.EventParam)
                {
                    x = cel.EventParam;
                }
                else if (x == cel.EventParam)
                {
                    x = cel.EventParam;
                    events.Remove(cel);
                }
            return events;
        }

        public int GetPreviousPlan(string signalID, DateTime startDate)
        {
            var endDate = startDate.AddHours(-12);
            var planRecord = _controllerEventLogRepository.GetSignalEventsByEventCode(signalID, startDate, endDate, 131);
            if (planRecord.Count() > 0)
                return planRecord.OrderByDescending(s => s.Timestamp).FirstOrDefault().EventParam;
            return 0;
        }
    }

}