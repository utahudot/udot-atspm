using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Enums
{
    [Flags]
    public enum SignalControllerType
    {
        Unknown = 0,
        ASC3 = 1 << 1,
        Cobalt = 1 << 2,
        ASC32070 = 1 << 3,
        MaxTime = 1 << 4,
        Trafficware = 1 << 5,
        SiemensSEPAC = 1 << 6,
        McCainATCEX = 1 << 7,
        Peek = 1 << 8,
        EOS = 1 << 9
    }
}
