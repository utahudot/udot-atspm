using ATSPM.Application.Business.Common;
using System.Collections.Generic;

namespace ATSPM.Application.Business.TurningMovementCounts
{

    namespace MOE.Common.Business.TMC
    {
        public class TurningMovementCountData
        {
            public TurningMovementCountData()
            {
                Volumes = new List<DataPointForInt>();
            }

            public string Direction { get; set; }

            public string MovementType { get; set; }

            public string LaneType { get; set; }

            public List<DataPointForInt> Volumes { get; set; }
        }
    }
}
