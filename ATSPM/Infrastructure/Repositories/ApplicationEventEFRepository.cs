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
            return _db.Set<ApplicationEvent>().ToList();
        }

        public ApplicationEvent GetApplicationEventByID(int applicationEventID)
        {
            return _db.Set<ApplicationEvent>().Where(ae => ae.Id == applicationEventID).FirstOrDefault();
        }

        public IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDates(DateTime StartDate, DateTime EndDate)
        {
            return _db.Set<ApplicationEvent>().Where(ae => ae.Timestamp > StartDate && ae.Timestamp <= EndDate).ToList();
        }

        public IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDatesByApplication(DateTime StartDate, DateTime EndDate, string ApplicationName)
        {
            return _db.Set<ApplicationEvent>().Where(ae => ae.Timestamp > StartDate && ae.Timestamp <= EndDate && ae.ApplicationName == ApplicationName).ToList();
        }

        public IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDatesByApplicationBySeverity(DateTime StartDate, DateTime EndDate, string ApplicationName, SeverityLevels Severity)
        {
            return _db.Set<ApplicationEvent>().Where(ae => ae.Timestamp > StartDate && ae.Timestamp <= EndDate && ae.ApplicationName == ApplicationName && ae.SeverityLevel == Severity).ToList();
        }

        public IReadOnlyCollection<ApplicationEvent> GetApplicationEventsBetweenDatesByClass(DateTime StartDate, DateTime EndDate, string ApplicationName, string ClassName)
        {
            return _db.Set<ApplicationEvent>().Where(ae => ae.Timestamp > StartDate && ae.Timestamp <= EndDate && ae.ApplicationName == ApplicationName && ae.Class == ClassName).ToList();
        }

        public IReadOnlyCollection<ApplicationEvent> GetEventsByDateDescriptions(DateTime startDate, DateTime endDate, List<string> descriptions)
        {
            return _db.Set<ApplicationEvent>().Where(ae => ae.Timestamp > startDate && ae.Timestamp <= endDate && descriptions.Contains(ae.Description)).ToList();
        }

        public void QuickAdd(string applicationName, string errorClass, string errorFunction, SeverityLevels severity, string description)
        {
            throw new NotImplementedException();
        }

        public void Remove(int id)
        {
            var g = _db.Set<ApplicationEvent>().Find(id);
            if (g != null)
            {
                _db.Set<ApplicationEvent>().Remove(g);
                _db.SaveChanges();
            }
        }
    }
}
