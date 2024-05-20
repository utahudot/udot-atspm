using ATSPM.Application.Business.Common;
using System.Collections.Generic;

namespace ATSPM.Application.Business.TimingAndActuation
{
    public class TimingAndActuationsOptions : OptionsBase
    {
        public List<short>? GlobalEventCodesList { get; set; }
        public List<short>? GlobalEventParamsList { get; set; }
        public List<short>? PhaseEventCodesList { get; set; }
    }
}
