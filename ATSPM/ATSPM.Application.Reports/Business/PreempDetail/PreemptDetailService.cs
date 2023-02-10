using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Application.Reports.ViewModels.ControllerEventLog;
using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using Legacy.Common.Business.Preempt;

namespace ATSPM.Application.Reports.Business.PreempDetail
{
    public class PreemptDetailService
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        public PreemptDetailService(IControllerEventLogRepository controllerEventLogRepository, ISignalRepository signalRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        public PreemptDetailResult GetChartData(PreemptDetailOptions preemptDetailOptions)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(preemptDetailOptions.SignalId, preemptDetailOptions.StartDate);
            var tables = new List<ControllerEventLogResult>();
            var preTestTables = new List<ControllerEventLogResult>();
            var eventsTable = FillforPreempt(preemptDetailOptions.SignalId, preemptDetailOptions.StartDate, preemptDetailOptions.EndDate);

            var t1 = new ControllerEventLogResult();
            var t2 = new ControllerEventLogResult();
            var t3 = new ControllerEventLogResult();
            var t4 = new ControllerEventLogResult();
            var t5 = new ControllerEventLogResult();
            var t6 = new ControllerEventLogResult();
            var t7 = new ControllerEventLogResult();
            var t8 = new ControllerEventLogResult();
            var t9 = new ControllerEventLogResult();
            var t10 = new ControllerEventLogResult();
            var t11 = new ControllerEventLogResult();
            var t12 = new ControllerEventLogResult();
            var t13 = new ControllerEventLogResult();
            var t14 = new ControllerEventLogResult();
            var t15 = new ControllerEventLogResult();
            var t16 = new ControllerEventLogResult();
            var t17 = new ControllerEventLogResult();
            var t18 = new ControllerEventLogResult();
            var t19 = new ControllerEventLogResult();
            var t20 = new ControllerEventLogResult();


            preTestTables.Add(t1);
            preTestTables.Add(t2);
            preTestTables.Add(t3);
            preTestTables.Add(t4);
            preTestTables.Add(t5);
            preTestTables.Add(t6);
            preTestTables.Add(t7);
            preTestTables.Add(t8);
            preTestTables.Add(t9);
            preTestTables.Add(t10);
            preTestTables.Add(t11);
            preTestTables.Add(t12);
            preTestTables.Add(t13);
            preTestTables.Add(t14);
            preTestTables.Add(t15);
            preTestTables.Add(t16);
            preTestTables.Add(t17);
            preTestTables.Add(t18);
            preTestTables.Add(t19);
            preTestTables.Add(t20);


            foreach (var row in eventsTable.Events)
                switch (row.EventParam)
                {
                    case 1:
                        t1.Events.Add(row);
                        break;
                    case 2:
                        t2.Events.Add(row);
                        break;
                    case 3:
                        t3.Events.Add(row);
                        break;
                    case 4:
                        t4.Events.Add(row);
                        break;
                    case 5:
                        t5.Events.Add(row);
                        break;
                    case 6:
                        t6.Events.Add(row);
                        break;
                    case 7:
                        t7.Events.Add(row);
                        break;
                    case 8:
                        t8.Events.Add(row);
                        break;
                    case 9:
                        t9.Events.Add(row);
                        break;
                    case 10:
                        t10.Events.Add(row);
                        break;
                    case 11:
                        t11.Events.Add(row);
                        break;
                    case 12:
                        t12.Events.Add(row);
                        break;
                    case 13:
                        t13.Events.Add(row);
                        break;
                    case 14:
                        t14.Events.Add(row);
                        break;
                    case 15:
                        t15.Events.Add(row);
                        break;
                    case 16:
                        t16.Events.Add(row);
                        break;
                    case 17:
                        t17.Events.Add(row);
                        break;
                    case 18:
                        t18.Events.Add(row);
                        break;
                    case 19:
                        t19.Events.Add(row);
                        break;
                    case 20:
                        t20.Events.Add(row);
                        break;
                }

            foreach (var t in preTestTables)
                TestForValidRecords(t, tables);

            foreach (var t in tables)
            {
                Add105Events(preemptDetailOptions.SignalId, preemptDetailOptions.StartDate, preemptDetailOptions.EndDate, t);
            }
            throw new NotImplementedException();
            //return new PreemptDetailResult(
            //    "Preempt Detail",
            //    signalId,
            //    signal.SignalDescription,
            //    startDate,
            //    endDate,


            //    );
        }

        public ControllerEventLogResult FillforPreempt(string signalID, DateTime startDate, DateTime endDate)
        {
            var codes = new List<int>();

            for (var i = 101; i <= 111; i++)
                codes.Add(i);

            var events = controllerEventLogRepository.GetSignalEventsByEventCodes(signalID, startDate, endDate, codes).OrderBy(c => c.Timestamp).ToList();
            return new ControllerEventLogResult(signalID, startDate, endDate, codes, events);
        }

        public void Add105Events(string signalId, DateTime startDate, DateTime endDate, ControllerEventLogResult existingEvents)
        {
            var events = controllerEventLogRepository.GetSignalEventsByEventCodes(signalId, startDate, endDate, new List<int> { 105, 111 });
            foreach (var v in events)
            {
                v.EventCode = 99;
                existingEvents.Events.Add(v);
            }
            existingEvents.Events = existingEvents.Events.OrderBy(e => e.Timestamp).ToList();
        }

        private void TestForValidRecords(ControllerEventLogResult t, List<ControllerEventLogResult> tables)
        {
            var AddToTables = false;

            if (t.Events.Count > 0)
            {
                var hasStart = from r in t.Events
                               where r.EventCode == 105 ||
                                     r.EventCode == 111 ||
                                     r.EventCode == 107
                               select r;

                if (hasStart.Any())
                    AddToTables = true;
            }

            if (AddToTables)
                tables.Add(t);
        }
    }
}