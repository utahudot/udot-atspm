﻿using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
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
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateOnly start, DateOnly end, Type dataType, int deviceId)
        {
            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end, deviceId))
                .Where(w => w.DataType == dataType)
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogs<T>> GetArchivedEvents<T>(string locationIdentifier, DateOnly start, DateOnly end, int deviceId) where T : EventLogModelBase
        {
            var type = typeof(T);

            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end, deviceId))
                .Where(w => w.DataType == type)
                .Cast<CompressedEventLogs<T>>()
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateOnly start, DateOnly end)
        {
            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end))
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateOnly start, DateOnly end, Type dataType)
        {
            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end))
                .Where(w => w.DataType == dataType)
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateOnly start, DateOnly end, int deviceId)
        {
            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end, deviceId))
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogs<T>> GetArchivedEvents<T>(string locationIdentifier, DateOnly start, DateOnly end) where T : EventLogModelBase
        {
            var type = typeof(T);

            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end))
                .Where(w => w.DataType == type)
                .Cast<CompressedEventLogs<T>>()
                .ToList();
        }

        #endregion
    }
}