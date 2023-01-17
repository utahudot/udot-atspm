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
    public static class DetectorRepositoryExtensions
    {
        #region Obsolete

        [Obsolete("This method is not used", true)]
        public static IReadOnlyList<Detector> GetDetectorsBySignalIDAndMetricType(this IDetectorRepository repo, string signalId, int metricId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("User CheckReportAvialbility(Detector detector, int metricId) instead", true)]
        public static bool CheckReportAvialbility(this IDetectorRepository repo, string detectorID, int metricId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("User GetList() instead", true)]
        public static Detector GetDetectorByDetectorID(this IDetectorRepository repo, string detectorId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("User GetList() instead", true)]
        public static IReadOnlyList<Detector> GetDetectorsBySignalID(this IDetectorRepository repo, string signalId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Lookup instead", true)]
        public static Detector GetDetectorByID(this IDetectorRepository repo, int id)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Add in the BaseClass", true)]
        public static Detector Add(this IDetectorRepository repo, Detector detector)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Update in the BaseClass", true)]
        public static void Update(this IDetectorRepository repo, Detector detector)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Remove in the BaseClass", true)]
        public static void Remove(this IDetectorRepository repo, Detector detector)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Remove in the BaseClass", true)]
        public static void Remove(this IDetectorRepository repo, int id)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetList instead")]
        public static IReadOnlyList<Detector> GetDetectorsByIds(this IDetectorRepository repo, List<int> excludedDetectorIds)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
