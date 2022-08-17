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
        IReadOnlyCollection<SpmwatchDogErrorEvent> GetAllSPMWatchDogErrorEvents();
        IReadOnlyCollection<SpmwatchDogErrorEvent> GetSPMWatchDogErrorEventsBetweenDates(DateTime StartDate, DateTime EndDate);
        SpmwatchDogErrorEvent GetSPMWatchDogErrorEventByID(int SPMWatchDogErrorEventID);
        [Obsolete("Use Update in the BaseClass")]
        void Update(SpmwatchDogErrorEvent SPMWatchDogErrorEvent);
        [Obsolete("Use Add in the BaseClass")]
        void AddListAndSaveToDatabase(List<SpmwatchDogErrorEvent> SPMWatchDogErrorEvents);
        [Obsolete("Use Add in the BaseClass")]
        void Add(SpmwatchDogErrorEvent SPMWatchDogErrorEvent);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(SpmwatchDogErrorEvent SPMWatchDogErrorEvent);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(int id);
    }
}
