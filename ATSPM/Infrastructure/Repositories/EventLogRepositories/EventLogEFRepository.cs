using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.EventLogRepositories
{
    ///<inheritdoc cref="IEventLogRepository"/>
    public class EventLogEFRepository : ATSPMRepositoryEFBase<CompressedEventLogBase>, IEventLogRepository
    {
        ///<inheritdoc/>
        public EventLogEFRepository(EventLogContext db, ILogger<EventLogEFRepository> log) : base(db, log) { }

        #region IEventLogRepository

        ///<inheritdoc/>
        public IReadOnlyList<EventLogModelBase> GetEvents(string locationIdentifier, DateOnly date)
        {
            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date)
                .ToList()
                .AddLocationIdentifer<EventLogModelBase>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<EventLogModelBase> GetEvents(string locationIdentifier, DateOnly date, int deviceId)
        {
            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DeviceId == deviceId)
                .AsEnumerable()
                .AddLocationIdentifer<EventLogModelBase>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<EventLogModelBase> GetEvents(string locationIdentifier, DateOnly date, Type dataType)
        {
            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DataType == dataType)
                .AsEnumerable()
                .AddLocationIdentifer<EventLogModelBase>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<T> GetEvents<T>(string locationIdentifier, DateOnly date) where T : EventLogModelBase
        {
            var type = typeof(T);

            return GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DataType == type)
                .AsEnumerable()
                .AddLocationIdentifer<T>();
        }

        ///<inheritdoc/>
        public IReadOnlyList<T> GetEvents<T>(string locationIdentifier, DateOnly date, int deviceId) where T : EventLogModelBase
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
