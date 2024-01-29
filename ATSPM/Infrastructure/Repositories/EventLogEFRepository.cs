using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
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
            return ProcessEventLogs(GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date)
                .AsEnumerable());
        }

        ///<inheritdoc/>
        public IReadOnlyList<AtspmEventModelBase> GetEvents(string locationIdentifier, DateOnly date, int deviceId)
        {
            return ProcessEventLogs(GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DeviceId == deviceId)
                .AsEnumerable());
        }

        ///<inheritdoc/>
        public IReadOnlyList<AtspmEventModelBase> GetEvents(string locationIdentifier, DateOnly date, Type dataType)
        {
            return ProcessEventLogs(GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DataType == dataType)
                .AsEnumerable());
        }

        ///<inheritdoc/>
        public IReadOnlyList<T> GetEvents<T>(string locationIdentifier, DateOnly date) where T : AtspmEventModelBase
        {
            var type = typeof(T);

            return ProcessEventLogs(GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DataType == type)
                .AsEnumerable())
                .Cast<T>()
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<T> GetEvents<T>(string locationIdentifier, DateOnly date, int deviceId) where T : AtspmEventModelBase
        {
            var type = typeof(T);

            return ProcessEventLogs(GetList()
                .Where(w => w.LocationIdentifier == locationIdentifier && w.ArchiveDate == date && w.DataType == type && w.DeviceId == deviceId)
                .AsEnumerable())
                .Cast<T>()
                .ToList();
        }

        #endregion

        private IReadOnlyList<AtspmEventModelBase> ProcessEventLogs(IEnumerable<CompressedEventsBase> items)
        {
            return items.SelectMany(m => m.Data, (o, r) =>
            {
                if (r is ILocationLayer l)
                    l.LocationIdentifier = o.LocationIdentifier;
                return r;
            }).ToList();
        }
    }
}
