using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IApplicationEventRepository : IAsyncRepository<ApplicationEvent>
    {
        IReadOnlyCollection<ApplicationEvent> GetAllApplicationEvents();
        IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDates(DateTime StartDate, DateTime EndDate);

        IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDatesByApplication(DateTime StartDate, DateTime EndDate,
            string ApplicationName);

        IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDatesByApplicationBySeverity(DateTime StartDate,
            DateTime EndDate, string ApplicationName, ApplicationEvent.SeverityLevels Severity);

        IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDatesByClass(DateTime StartDate, DateTime EndDate,
            string ApplicationName, string ClassName);

        ApplicationEvent GetApplicationEventByID(int applicationEventID);
        [Obsolete("Use Update in the BaseClass")]
        void Update(ApplicationEvent applicationEvent);
        [Obsolete("Use Add in the BaseClass")]
        void Add(ApplicationEvent applicationEvent);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(ApplicationEvent applicationEvent);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(int id);
        [Obsolete("Use Add in the BaseClass")]
        void QuickAdd(string applicationName, string errorClass, string errorFunction,
            ApplicationEvent.SeverityLevels severity, string description);

        IReadOnlyCollection<ApplicationEvent> GetEventsByDateDescriptions(DateTime startDate, DateTime endDate,
            List<string> descriptions);
    }
}
