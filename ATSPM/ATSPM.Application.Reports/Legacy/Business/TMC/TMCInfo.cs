using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Legacy.Common.Business.TMC
{
    public class TMCInfo
    {
        [DataMember] public List<string> ImageLocations;

        [DataMember] public List<TMCData> tmcData;

        public TMCInfo()
        {
            ImageLocations = new List<string>();
            tmcData = new List<TMCData>();
        }
    }
}