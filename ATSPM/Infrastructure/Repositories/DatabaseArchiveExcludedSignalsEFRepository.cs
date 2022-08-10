using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public class DatabaseArchiveExcludedSignalsEFRepository : ATSPMRepositoryEFBase<DatabaseArchiveExcludedSignal>, IDatabaseArchiveExcludedSignalsRepository
    {
        public DatabaseArchiveExcludedSignalsEFRepository(DbContext db, ILogger<DatabaseArchiveExcludedSignalsEFRepository> log) : base(db, log)
        {

        }

        public void AddToExcludedList(string signalId)
        {
            throw new NotImplementedException();
        }

        public void DeleteFromExcludedList(string signalId)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string signalId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<DatabaseArchiveExcludedSignal> GetAllExcludedSignals()
        {
            throw new NotImplementedException();
        }

        public DatabaseArchiveExcludedSignal GetExcludedSignalBySignalId(string signalId)
        {
            throw new NotImplementedException();
        }
    }
}
