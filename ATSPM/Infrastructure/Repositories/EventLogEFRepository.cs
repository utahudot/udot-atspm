using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Data.Models.EventModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    ///<inheritdoc cref="IEventLogRepository"/>
    public class EventLogEFRepository : ATSPMRepositoryEFBase<CompressedEventsBase>, IEventLogRepository
    {
        ///<inheritdoc/>
        public EventLogEFRepository(EventLogContext db, ILogger<EventLogEFRepository> log) : base(db, log) { }

        #region IEventLogRepository

        ///<inheritdoc/>
        public IReadOnlyList<AtspmEventModelBase> GetEvents(string locationIdentifier, DateOnly date)
        {
            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date)
                .ToList()
                .AddLocationIdentifer<AtspmEventModelBase>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<AtspmEventModelBase> GetEvents(string locationIdentifier, DateOnly date, int deviceId)
        {
            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DeviceId == deviceId)
                .AsEnumerable()
                .AddLocationIdentifer<AtspmEventModelBase>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<AtspmEventModelBase> GetEvents(string locationIdentifier, DateOnly date, Type dataType)
        {
            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DataType == dataType)
                .AsEnumerable()
                .AddLocationIdentifer<AtspmEventModelBase>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<T> GetEvents<T>(string locationIdentifier, DateOnly date) where T : AtspmEventModelBase
        {
            var type = typeof(T);

            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DataType == type)
                .AsEnumerable()
                .AddLocationIdentifer<T>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<T> GetEvents<T>(string locationIdentifier, DateOnly date, int deviceId) where T : AtspmEventModelBase
        {
            var type = typeof(T);

            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DataType == type && w.DeviceId == deviceId)
                .AsEnumerable()
                .AddLocationIdentifer<T>();
        }

        #endregion
    }
}
