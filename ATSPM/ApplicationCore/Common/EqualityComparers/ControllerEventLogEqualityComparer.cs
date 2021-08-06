using ControllerLogger.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ControllerLogger.Application.Common.EqualityComparers
{
    public class ControllerEventLogEqualityComparer : EqualityComparer<ControllerEventLog>
    {
        public override bool Equals([AllowNull] ControllerEventLog x, [AllowNull] ControllerEventLog y)
        {
            return (x.Timestamp.Ticks == y.Timestamp.Ticks && x.EventCode == y.EventCode && x.EventParam == y.EventParam);
        }

        public override int GetHashCode([DisallowNull] ControllerEventLog obj)
        {
            var h = obj.Timestamp.Ticks + obj.EventCode + obj.EventParam;
            return h.GetHashCode();
        }
    }
}
