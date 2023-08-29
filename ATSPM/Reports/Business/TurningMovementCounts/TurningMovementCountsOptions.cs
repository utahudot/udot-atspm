using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.TurningMovementCounts
{
    public class TurningMovementCountsOptions
    {

        //public TurningMovementCountsOptions(int approachId, DateTime startDate, DateTime endDate, 
        //    int binSize)
        //{
        //    ApproachId = approachId;
        //    Start = startDate;
        //    End = endDate;
        //    SelectedBinSize = binSize;
        //}
        public int SelectedBinSize { get; set; }
        public int ApproachId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public LaneTypes LaneType { get; set; }
        public List<MovementTypes> MovementTypes { get; set; }
    }
}