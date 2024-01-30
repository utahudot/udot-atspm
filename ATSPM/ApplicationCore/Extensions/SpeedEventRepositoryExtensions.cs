using ATSPM.Application.Repositories.EventLogRepositories;
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
    public static class SpeedEventRepositoryExtensions
    {
        public static IReadOnlyList<OldSpeedEvent> GetSpeedEventsByDetector(this ISpeedEventRepository repo, Detector detector, DateTime startDate, DateTime endDate, int minSpeedFilter = 5)
        {
            return repo.GetList()
                .Where(w => w.TimeStamp > startDate && w.TimeStamp < endDate && w.Mph > minSpeedFilter)
                .ToList();
        }

        #region Obsolete

        [Obsolete("This method isn't currently being used")]
        public static IReadOnlyList<OldSpeedEvent> GetSpeedEventsByLocation(this ISpeedEventRepository repo, DateTime startDate, DateTime endDate, Approach approach)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
