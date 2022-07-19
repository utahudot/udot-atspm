using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Models
{
    public partial class ApproachCycleAggregation : Aggregation
    {
        public int Id { get; set; }
        public override DateTime BinStartTime { get; set; }
        public int ApproachId { get; set; }
        public double RedTime { get; set; }
        public double YellowTime { get; set; }
        public double GreenTime { get; set; }
        public int TotalCycles { get; set; }
        public int PedActuations { get; set; }
        public bool IsProtectedPhase { get; set; }

        //public sealed class ApproachCycleAggregationClassMap : ClassMap<ApproachCycleAggregation>
        //{
        //    public ApproachCycleAggregationClassMap()
        //    {
        //        //Map(m => m.Approach).Ignore();
        //        Map(m => m.Id).Name("Record Number");
        //        Map(m => m.BinStartTime).Name("Bin Start Time");
        //        Map(m => m.ApproachId).Name("Approach ID");
        //        Map(m => m.RedTime).Name("RedTime");
        //        Map(m => m.YellowTime).Name("YellowTime");
        //        Map(m => m.GreenTime).Name("GreenTime");
        //        Map(m => m.TotalCycles).Name("Total Cycles");
        //        Map(m => m.PedActuations).Name("Ped Actuations");
        //        Map(m => m.IsProtectedPhase).Name("Is Protected Phase");
        //    }
        //}
    }
}
