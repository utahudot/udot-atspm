using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IAggregationRepository"/>
    public class AggregationEFRepository : ATSPMRepositoryEFBase<CompressedAggregationBase>, IAggregationRepository
    {
        ///<inheritdoc/>
        public AggregationEFRepository(AggregationContext db, ILogger<AggregationEFRepository> log) : base(db, log) { }

        #region IAggregationRepository

        ///<inheritdoc/>
        public IReadOnlyList<AggregationModelBase> GetAggregations(string locationIdentifier, DateOnly date)
        {
            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date)
                .AsEnumerable()
                .AddLocationIdentifer<AggregationModelBase>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<AggregationModelBase> GetAggregations(string locationIdentifier, DateOnly date, Type dataType)
        {
            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DataType == dataType)
                .AsEnumerable()
                .AddLocationIdentifer<AggregationModelBase>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<T> GetAggregations<T>(string locationIdentifier, DateOnly date) where T : AggregationModelBase
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
