using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class SpeedEventRepositoryExtensions
    {
        public static IReadOnlyList<SpeedEvent> GetSpeedEventsByDetector(this ISpeedEventLogRepository repo, Detector detector, DateTime startDate, DateTime endDate, int minSpeedFilter = 5)
        {
            throw new NotImplementedException();
        }

        #region Obsolete

        [Obsolete("This method isn't currently being used")]
        public static IReadOnlyList<SpeedEvent> GetSpeedEventsByLocation(this ISpeedEventLogRepository repo, DateTime startDate, DateTime endDate, Approach approach)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
