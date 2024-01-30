using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.EventLogRepositories
{
    ///<inheritdoc cref="IIndiannaEventLogRepository"/>
    public class IndiannaEventLogEFRepository : ATSPMRepositoryEFBase<CompressedEventLogs<IndianaEvent>>, IIndiannaEventLogRepository
    {
        ///<inheritdoc/>
        public IndiannaEventLogEFRepository(EventLogContext db, ILogger<IndiannaEventLogEFRepository> log) : base(db, log) { }

        #region IIndiannaEventRepository

        ///<inheritdoc/>
        public IReadOnlyList<IndianaEvent> GetEventsBetweenDates(string locationId, DateTime startTime, DateTime endTime)
        {
            var result = table
                .FromSpecification(new EventLogDateRangeSpecification(locationId, startTime, endTime))
                .AsNoTracking()
                .AsEnumerable()
                .AddLocationIdentifer<IndianaEvent>()
                .FromSpecification(new EventLogDateTimeRangeSpecification(locationId, startTime, endTime))
                .Cast<IndianaEvent>()
                .ToList();

            return result;
        }

        #endregion
    }
}
