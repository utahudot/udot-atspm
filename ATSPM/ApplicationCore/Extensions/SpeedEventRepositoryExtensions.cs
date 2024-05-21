using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Extensions
{
    public static class SpeedEventRepositoryExtensions
    {
        public static IReadOnlyList<SpeedEvent> GetSpeedEventsByDetector(this ISpeedEventLogRepository repo, string locationIdentifier, Detector detector, DateTime start, DateTime end, int minSpeedFilter = 5)
        {
            return repo.GetEventsBetweenDates(locationIdentifier, start, end)
                .Where(e => e.DetectorId == detector.DectectorIdentifier
                && e.Mph > minSpeedFilter
                ).ToList();

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
