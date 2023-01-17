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
    public class PlansBase
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public List<ControllerEventLog> Events { get; set; }
        public PlansBase(string signalID, DateTime startDate, DateTime endDate, IControllerEventLogRepository controllerEventLogRepository)
        {
            Events = controllerEventLogRepository.GetSignalEventsByEventCode(signalID, startDate, endDate, 131).ToList();
            //Get the plan Previous to the start date
            //if(this.Events.Count > 0)
            //{
            var tempEvent = new ControllerEventLog();
            tempEvent.SignalId = signalID;
            tempEvent.Timestamp = startDate;
            tempEvent.EventCode = 131;
            tempEvent.EventParam = GetPreviousPlan(signalID, startDate);

            Events.Insert(0, tempEvent);
            //}

            //Remove Duplicate Plans
            var x = -1;
            var temp = new List<ControllerEventLog>();
            foreach (var cel in Events)
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
                    Events.Remove(cel);
                }

            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        public int GetPreviousPlan(string signalID, DateTime startDate)
        {
            var endDate = startDate.AddHours(-12);
            var planRecord = controllerEventLogRepository.GetSignalEventsByEventCode(signalID, startDate, endDate, 131);
            if (planRecord.Count() > 0)
                return planRecord.OrderByDescending(s => s.Timestamp).FirstOrDefault().EventParam;
            return 0;
        }
    }
}