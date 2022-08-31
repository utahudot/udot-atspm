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
            try
            {
                _db.Set<SpmwatchDogErrorEvent>().AddRange(SPMWatchDogErrorEvents);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                //var er =
                //    ApplicationEventRepositoryFactory.Create();
                //er.QuickAdd("MOE.Common", "SPMWatchDogErrrorEventRepository", "AddListAndSaveToDatabase",
                //    ApplicationEvent.SeverityLevels.Medium, ex.Message);
            }
        }

        public IReadOnlyCollection<SpmwatchDogErrorEvent> GetAllSPMWatchDogErrorEvents()
        {
            return _db.Set<SpmwatchDogErrorEvent>().ToList();
        }

        public SpmwatchDogErrorEvent GetSPMWatchDogErrorEventByID(int SPMWatchDogErrorEventID)
        {
            return _db.Set<SpmwatchDogErrorEvent>().Where(w => w.Id == SPMWatchDogErrorEventID).FirstOrDefault();
        }

        public IReadOnlyCollection<SpmwatchDogErrorEvent> GetSPMWatchDogErrorEventsBetweenDates(DateTime StartDate, DateTime EndDate)
        {
            return _db.Set<SpmwatchDogErrorEvent>().Where(w => w.TimeStamp >= StartDate && w.TimeStamp < EndDate).ToList();
        }

        public void Remove(int id)
        {
            var g = GetSPMWatchDogErrorEventByID(id);
            if (g != null)
            {
                _db.Set<SpmwatchDogErrorEvent>().Remove(g);
                _db.SaveChanges();
            }
        }
    }
}
