using ATSPM.Data.Models;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ATSPM.Data.Models.EventModels;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Device event log repository
    /// </summary>
    public interface IEventLogRepository : IAsyncRepository<CompressedEventsBase>
    {
        IReadOnlyList<AtspmEventModelBase> GetEvents(string locationIdentifier, DateOnly date);

        IReadOnlyList<AtspmEventModelBase> GetEvents(string locationIdentifier, DateOnly date, int deviceId);

        IReadOnlyList<AtspmEventModelBase> GetEvents(string locationIdentifier, DateOnly date, Type dataType);

        IReadOnlyList<T> GetEvents<T>(string locationIdentifier, DateOnly date) where T : AtspmEventModelBase;

        IReadOnlyList<T> GetEvents<T>(string locationIdentifier, DateOnly date, int deviceId) where T : AtspmEventModelBase;
    }
}
