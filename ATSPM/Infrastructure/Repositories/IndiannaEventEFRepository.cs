using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventModels;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    ///<inheritdoc cref="IIndiannaEventRepository"/>
    public class IndiannaEventEFRepository : ATSPMRepositoryEFBase<CompressedEvents<IndiannaEvent>>, IIndiannaEventRepository
    {
        ///<inheritdoc/>
        public IndiannaEventEFRepository(EventLogContext db, ILogger<IndiannaEventEFRepository> log) : base(db, log) { }

        #region IIndiannaEventRepository

        ///<inheritdoc/>
        public IReadOnlyList<IndiannaEvent> GetEventsBetweenDates(string locationId, DateTime startTime, DateTime endTime)
        {
            var result = table
                .FromSpecification(new EventLogDateRangeSpecification(locationId, startTime, endTime))
                .AsNoTracking()
                .AsEnumerable()
                .AddLocationIdentifer<IndiannaEvent>()
                .FromSpecification(new EventLogDateTimeRangeSpecification(locationId, startTime, endTime))
                .Cast<IndiannaEvent>()
                .ToList();

            return result;
        }

        #endregion
    }
}
