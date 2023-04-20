using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application
{
    /// <summary>
    /// Equations used in the ATSPM engine as defined by Measures and Assumptions
    /// </summary>
    public static class AtspmMath
    {

        /// <summary>
        /// Calculates the <see cref="TimeSpan"/> difference between the first and second <see cref="DataLoggerEnum"/>
        /// <list type="bullet">
        /// <listheader>Used in the following steps of Preemption Details:</listheader>
        /// <item>
        /// <term>Equation 52</term>
        /// <description>Step 1: Calculate Dwell Time</description>
        /// </item>
        /// <item>
        /// <term>Equation 53</term>
        /// <description>Step 2: Calculate Track Clear Time</description>
        /// </item>
        /// <item>
        /// <term>Equation 54</term>
        /// <description>Step 3: Calculate Time to Service</description>
        /// </item>
        /// <item>
        /// <term>Equation 55</term>
        /// <description>Step 4: Calculate Delay</description>
        /// </item>
        /// <item>
        /// <term>Equation 56</term>
        /// <description>Step 5: Calculate Time to Gate Down</description>
        /// </item>
        /// <item>
        /// <term>Equation 57</term>
        /// <description>Step 6: Calculate Time to Call Max Out</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="items"><see cref="ControllerEventLog"/> list the <see cref="DataLoggerEnum"/> are sorted</param>
        /// <param name="first">Starting event code</param>
        /// <param name="second">Ending event code</param>
        /// <returns><see cref="TimeSpan"/> difference of <paramref name="second"/> minus <paramref name="first"/></returns>
        public static IEnumerable<Tuple<ControllerEventLog[], TimeSpan>> TimeSpanFromConsecutiveCodes(this IEnumerable<ControllerEventLog> items, DataLoggerEnum first, DataLoggerEnum second)
        {
            var preFilter = items.OrderBy(o => o.Timestamp)
                .Where(w => w.EventCode == (int)first || w.EventCode == (int)second)
                //.Where(w => w.Timestamp > DateTime.MinValue && w.Timestamp < DateTime.MaxValue)
                .ToList();

            var result = preFilter.Where((x, y) =>
                    (y < preFilter.Count() - 1 && x.EventCode == (int)first && preFilter[y + 1].EventCode == (int)second) ||
                    (y > 0 && x.EventCode == (int)second && preFilter[y - 1].EventCode == (int)first))
                        .Chunk(2)
                        .Select(l => new Tuple<ControllerEventLog[], TimeSpan>(new ControllerEventLog[] { l[0], l[1] }, l[1].Timestamp - l[0].Timestamp));

            return result;
        }

        /// <summary>
        /// Identify and Adjust Vehicle Activations
        /// <list type="bullet">
        /// <listheader>Used in the following Measures and Assumptions:</listheader>
        /// <item>
        /// <term>Equation 1</term>
        /// <description>Used in step 1 of Approach Volume</description>
        /// </item>
        /// <item>
        /// <term>Equation 27</term>
        /// <description>Used in step 1 of Approach Delay</description>
        /// </item>
        /// <item>
        /// <term>Equation 44</term>
        /// <description>Used in step 1 of Purdue Coordination Diagram</description>
        /// </item>
        /// <item>
        /// <term>Equation 64</term>
        /// <description>Used in step 1 of Arrivals on Red</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="timestamp">time stamp of arrival time (EC 82)</param>
        /// <param name="approachSpeed">stated speed for the approach</param>
        /// <param name="distanceFromStopBar">stated distance to stop bar</param>
        /// <param name="latencyCorrection">latency correction, based on system latency factors</param>
        /// <returns>adjusted time stamp of arrival time</returns>
        public static DateTime AdjustTimeStamp(DateTime timestamp, int approachSpeed, int distanceFromStopBar, double latencyCorrection = 0)
        {
            return timestamp.AddSeconds(distanceFromStopBar / (approachSpeed * 1.467)).AddSeconds(latencyCorrection * -1);
        }
    }
}
