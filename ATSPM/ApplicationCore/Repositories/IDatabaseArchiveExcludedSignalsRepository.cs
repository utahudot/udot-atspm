using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IDatabaseArchiveExcludedSignalsRepository : IAsyncRepository<DatabaseArchiveExcludedSignal>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<DatabaseArchiveExcludedSignal> GetAllExcludedSignals();
        [Obsolete("Use Lookup instead")]
        DatabaseArchiveExcludedSignal GetExcludedSignalBySignalId(string signalId);
        [Obsolete("Use Delete in the BaseClass")]
        void DeleteFromExcludedList(string signalId);
        [Obsolete("Use Add in the BaseClass")]
        void AddToExcludedList(string signalId);
        bool Exists(string signalId);
    }
}
