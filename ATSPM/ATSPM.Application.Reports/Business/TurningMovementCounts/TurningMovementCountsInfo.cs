using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.TurningMovementCounts
{
    public class TurningMovementCountsInfo
    {
        [DataMember] public List<string> ImageLocations;

        [DataMember] public List<TurningMovementCountsData> tmcData;

        public TurningMovementCountsInfo()
        {
            ImageLocations = new List<string>();
            tmcData = new List<TurningMovementCountsData>();
        }
    }
}