using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    ///<inheritdoc cref="IAggregationRepository"/>
    public class AggregationEFRepository : ATSPMRepositoryEFBase<CompressedAggregationBase>, IAggregationRepository
    {
        ///<inheritdoc/>
        public AggregationEFRepository(AggregationContext db, ILogger<AggregationEFRepository> log) : base(db, log) { }

        #region IAggregationRepository

        ///<inheritdoc/>
        public IReadOnlyList<AtspmAggregationModelBase> GetAggregations(string locationIdentifier, DateOnly date)
        {
            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date)
                .AsEnumerable()
                .AddLocationIdentifer<AtspmAggregationModelBase>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<AtspmAggregationModelBase> GetAggregations(string locationIdentifier, DateOnly date, Type dataType)
        {
            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DataType == dataType)
                .AsEnumerable()
                .AddLocationIdentifer<AtspmAggregationModelBase>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<T> GetAggregations<T>(string locationIdentifier, DateOnly date) where T : AtspmAggregationModelBase
        {
            var type = typeof(T);

            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DataType == type)
                .AsEnumerable()
                .AddLocationIdentifer<T>();
        }

        #endregion
    }
}
