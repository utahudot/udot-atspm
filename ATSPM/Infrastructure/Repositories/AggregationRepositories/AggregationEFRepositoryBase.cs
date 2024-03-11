using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    /// <summary>
    /// Generic base for accessing aggregations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AggregationEFRepositoryBase<T> : ATSPMRepositoryEFBase<CompressedAggregations<T>>, IAggregationRepository<T> where T : AggregationModelBase
    {
        ///<inheritdoc/>
        public AggregationEFRepositoryBase(AggregationContext db, ILogger<AggregationEFRepositoryBase<T>> log) : base(db, log) { }

        #region IAggregationRepository

        ///<inheritdoc/>
        public virtual IReadOnlyList<T> GetAggregationsBetweenDates(string locationId, DateTime startTime, DateTime endTime)
        {
            var result = table
                .FromSpecification(new AggregationDateRangeSpecification(locationId, DateOnly.FromDateTime(startTime), DateOnly.FromDateTime(endTime)))
                .AsNoTracking()
                .AsEnumerable()
                .SelectMany(m => m.Data)
                .FromSpecification(new AggregationDateTimeRangeSpecification(locationId, startTime, endTime))
                .Cast<T>()
                .ToList();

            return result;
        }

        #endregion
    }
}
