using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ATSPM.Application.Repositories
{
    public interface ISPMWatchDogErrorEventRepository : IAsyncRepository<SpmwatchDogErrorEvent>
    {
        IReadOnlyCollection<SPMWatchDogErrorEvent> GetAllSPMWatchDogErrorEvents();
        IReadOnlyCollection<SPMWatchDogErrorEvent> GetSPMWatchDogErrorEventsBetweenDates(DateTime StartDate, DateTime EndDate);
        SPMWatchDogErrorEvent GetSPMWatchDogErrorEventByID(int SPMWatchDogErrorEventID);
        [Obsolete("Use Update in the BaseClass")]
        void Update(SPMWatchDogErrorEvent SPMWatchDogErrorEvent);
        [Obsolete("Use Add in the BaseClass")]
        void AddListAndSaveToDatabase(List<SPMWatchDogErrorEvent> SPMWatchDogErrorEvents);
        [Obsolete("Use Add in the BaseClass")]
        void Add(SPMWatchDogErrorEvent SPMWatchDogErrorEvent);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(SPMWatchDogErrorEvent SPMWatchDogErrorEvent);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(int id);
    }
}
