using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class DetectorExtensions
    {
        public static double GetOffset(this Detector detector)
        {
            if (detector.DecisionPoint == null)
                detector.DecisionPoint = 0;
            if (detector.Approach.Mph.HasValue && detector.Approach.Mph > 0)
            {
                return Convert.ToDouble((detector.DistanceFromStopBar / (detector.Approach.Mph * 1.467) - detector.DecisionPoint) * 1000);
            }
            else
            {
                return 0;
            }

        }

        public static bool DetectorSupportsThisMetric(this Detector detector, int metricId)
        {
            var result = false;
            if (detector.DetectionTypes != null)
            {
                foreach (var dt in detector.DetectionTypes)
                {
                    foreach (var m in dt.MetricTypeMetrics)
                    {
                        if (m.Id == metricId)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public static bool CheckReportAvialbility(this Detector detector, int metricId)
        {
            return detector.DetectionTypes.SelectMany(s => s.MetricTypeMetrics).Any(a => a.Id == metricId);
        }
    }
}
