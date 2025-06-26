using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Extensions
{
    public static class IndianaEventExtensions
    {
        public static IList<IndianaEvent> KeepFirstSequentialEvent(this IEnumerable<IndianaEvent> events, IndianaEnumerations eventCode)
        {
            var sort = events.OrderBy(o => o.Timestamp).ToList();

            return sort.Where((w, i) => i == 0 || w.EventCode != (int)eventCode || (w.EventCode == (int)eventCode && w.EventCode != sort[i - 1].EventCode)).ToList();
        }

        public static ILookup<string, IndianaEvent> GroupEventsByParamType(this IEnumerable<IndianaEvent> events)
        {
            var codeToParamType = Enum.GetValues(typeof(IndianaEnumerations))
                .Cast<IndianaEnumerations>()
                .ToDictionary(
                    e => (short)e,
                    e => e.GetAttributeOfType<IndianaEventLayerAttribute>()?.IndianaEventParamType.ToString() ?? IndianaEventParamType.None.ToString()
                );

            return events
                .Where(e => codeToParamType.ContainsKey(e.EventCode))
                .ToLookup(e => codeToParamType[e.EventCode]);
        }

    }
}
