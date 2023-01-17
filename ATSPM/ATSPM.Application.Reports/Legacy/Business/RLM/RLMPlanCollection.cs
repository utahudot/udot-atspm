using System;
using System.Collections.Generic;
using System.Linq;
using Legacy.Common.Models;

namespace Legacy.Common.Business
{
    public class RLMPlanCollection
    {
        /// <summary>
        ///     Default Constructor Used for PCDs
        /// </summary>
        /// <param name="cycleEvents"></param>
        /// <param name="detectortable"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="signalId"></param>
        /// <param name="region"></param>
        public RLMPlanCollection(List<RLMCycle> cycles, DateTime startdate,
            DateTime enddate, double srlvSeconds, Approach approach, SPM db)
        {
            Approach = approach;
            SRLVSeconds = srlvSeconds;
            GetPlanCollection(startdate, enddate, cycles, db);
        }


        public double Violations
        {
            get { return PlanList.Sum(d => d.Violations); }
        }

        public List<RLMPlan> PlanList { get; } = new List<RLMPlan>();

        public double SRLVSeconds { get; }

        public Approach Approach { get; set; }


        public void GetPlanCollection(DateTime startDate, DateTime endDate,
            List<RLMCycle> cycles, SPM db)
        {
            var ds =
                new ControllerEventLogs(Approach.SignalID, startDate, endDate, new List<int> {131}, db);
            var row = new Controller_Event_Log();
            row.Timestamp = startDate;
            row.SignalID = Approach.SignalID;
            row.EventCode = 131;
            try
            {
                row.EventParam = GetPreviousPlan(Approach.SignalID, startDate);

                ds.Events.Insert(0, row);
            }
            catch
            {
                row.EventParam = 0;
                ds.Events.Insert(0, row);
            }
            // remove duplicate plan entries
            ds.MergeEvents(ds);
            for (var i = 0; i < ds.Events.Count(); i++)
                //if this is the last plan then we want the end of the plan
                //to cooincide with the end of the graph
                if (ds.Events.Count() - 1 == i)
                {
                    if (ds.Events[i].Timestamp != endDate)
                    {
                        var plan = new RLMPlan(ds.Events[i].Timestamp, endDate, ds.Events[i].EventParam,
                            cycles, SRLVSeconds, Approach);
                        AddItem(plan);
                    }
                }
                //else we add the plan with the next plans' time stamp as the end of the plan
                else
                {
                    if (ds.Events[i].Timestamp != ds.Events[i + 1].Timestamp)
                    {
                        var plan = new RLMPlan(ds.Events[i].Timestamp,
                            ds.Events[i + 1].Timestamp, ds.Events[i].EventParam, cycles, SRLVSeconds, Approach);
                        AddItem(plan);
                    }
                }
        }

        public void AddItem(RLMPlan item)
        {
            PlanList.Add(item);
        }

        public static int GetPreviousPlan(string signalID, DateTime startDate)
        {
            var db = new SPM();
            var endDate = startDate.AddHours(-12);
            var planRecord = from r in db.Controller_Event_Log
                             where r.SignalID == signalID &&
                                   r.Timestamp >= endDate &&
                                   r.Timestamp <= startDate &&
                                   r.EventCode == 131
                             select r;
            if (planRecord.Count() > 0)
                return planRecord.OrderByDescending(s => s.Timestamp).FirstOrDefault().EventParam;
            return 0;
        }
    }
}