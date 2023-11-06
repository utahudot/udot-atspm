using ATSPM.Application.ValueObjects;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ATSPM.Application.Common.EqualityComparers
{
    public class ControllerEventLogEqualityComparer : EqualityComparer<ControllerEventLog>
    {
        public override bool Equals([AllowNull] ControllerEventLog x, [AllowNull] ControllerEventLog y)
        {
            DateTime.TryParse(x?.Timestamp.ToString("MM-dd-yyyy HH:mm:ss.f"), out DateTime timeX);
            DateTime.TryParse(y?.Timestamp.ToString("MM-dd-yyyy HH:mm:ss.f"), out DateTime timeY);

            return x.SignalIdentifier == y.SignalIdentifier && timeX.Ticks == timeY.Ticks && x.EventCode == y.EventCode && x.EventParam == y.EventParam;
        }

        public override int GetHashCode([DisallowNull] ControllerEventLog obj)
        {
            DateTime.TryParse(obj.Timestamp.ToString("MM-dd-yyyy HH:mm:ss.f"), out DateTime time);

            var h = obj.SignalIdentifier.GetHashCode() + time.Ticks + obj.EventCode + obj.EventParam;

            return h.GetHashCode();
        }
    }
}
