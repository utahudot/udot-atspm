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
    public class SPMWatchDogErrorEventEFRepository : ATSPMRepositoryEFBase<SpmwatchDogErrorEvent>, ISPMWatchDogErrorEventRepository
    {
        public SPMWatchDogErrorEventEFRepository(DbContext db, ILogger<SPMWatchDogErrorEventEFRepository> log) : base(db, log)
        {

        }

        public void AddListAndSaveToDatabase(List<SpmwatchDogErrorEvent> SPMWatchDogErrorEvents)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<SpmwatchDogErrorEvent> GetAllSPMWatchDogErrorEvents()
        {
            throw new NotImplementedException();
        }

        public SpmwatchDogErrorEvent GetSPMWatchDogErrorEventByID(int SPMWatchDogErrorEventID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<SpmwatchDogErrorEvent> GetSPMWatchDogErrorEventsBetweenDates(DateTime StartDate, DateTime EndDate)
        {
            throw new NotImplementedException();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
