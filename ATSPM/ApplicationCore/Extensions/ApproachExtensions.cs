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
    public static class ApproachExtensions
    {
        public static List<Detector> GetDetectorsForMetricType(this Approach approach, int metricTypeID)
        {
            var detectorsForMetricType = new List<Detector>();
            if (approach.Detectors != null)
            {
                foreach (var d in approach.Detectors)
                {
                    if (d.DetectorSupportsThisMetric(metricTypeID))
                    {
                        detectorsForMetricType.Add(d);
                    }
                }
            }
            return detectorsForMetricType;
        }

        public static List<Detector> GetAllDetectorsOfDetectionType(this Approach approach, DetectionTypes detectionType)
        {
            if (approach.Detectors != null)
            {
                List<Detector> result = new List<Detector>();
                foreach (var d in approach.Detectors)
                    if (d.DetectionTypes  != null)
                    {
                        var detectionTypes = d.DetectionTypes.Select(t => t.Id).ToList();
                        if(detectionTypes.Contains(detectionType))
                        {
                            result.Add(d);
                        }
                    }
                return result;
            }
            return new List<Detector>();
        }
    }
}
