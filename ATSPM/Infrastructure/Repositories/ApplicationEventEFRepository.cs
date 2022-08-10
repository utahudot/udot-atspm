using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ATSPM.Application.Enums;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public class ApplicationEventEFRepository : ATSPMRepositoryEFBase<ApplicationEvent>, IApplicationEventRepository
    {
        public ApplicationEventEFRepository(DbContext db, ILogger<ApplicationEventEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<ApplicationEvent> GetAllApplicationEvents()
        {
            throw new NotImplementedException();
        }

        public ApplicationEvent GetApplicationEventByID(int applicationEventID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDates(DateTime StartDate, DateTime EndDate)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDatesByApplication(DateTime StartDate, DateTime EndDate, string ApplicationName)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDatesByApplicationBySeverity(DateTime StartDate, DateTime EndDate, string ApplicationName, SeverityLevels Severity)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDatesByClass(DateTime StartDate, DateTime EndDate, string ApplicationName, string ClassName)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<ApplicationEvent> GetEventsByDateDescriptions(DateTime startDate, DateTime endDate, List<string> descriptions)
        {
            throw new NotImplementedException();
        }

        public void QuickAdd(string applicationName, string errorClass, string errorFunction, SeverityLevels severity, string description)
        {
            throw new NotImplementedException();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
