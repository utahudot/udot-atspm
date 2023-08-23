using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Application.ValueObjects;
using ATSPM.Data;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public class SignalEFRepository : ATSPMRepositoryEFBase<Signal>, ISignalRepository
    {
        public SignalEFRepository(ConfigContext db, ILogger<SignalEFRepository> log) : base(db, log) { }

        private IQueryable<Signal> BaseQuery()
        {
            return base.GetList()
                .Include(i => i.ControllerType)
                .Include(i => i.Jurisdiction)
                .Include(i => i.Region)
                .Include(i => i.VersionAction)
                .Include(i => i.Approaches)
                .ThenInclude(i => i.Detectors)
                .Include(i => i.Areas);
            //.Include(i => i.MetricComments);
        }

        #region Overrides

        //public override IQueryable<Signal> GetList()
        //{
        //    return base.GetList()
        //        .Include(i => i.ControllerType)
        //        .Include(i => i.Jurisdiction)
        //        .Include(i => i.Region)
        //        .Include(i => i.VersionAction)
        //        .Include(i => i.Approaches)
        //        .ThenInclude(i => i.Detectors)
        //        .Include(i => i.Areas);
        //    //.Include(i => i.MetricComments);
        //}

        #endregion

        #region ISignalRepository

        public IReadOnlyList<Signal> GetAllVersionsOfSignal(string signalId)
        {
            var result = BaseQuery()
                .FromSpecification(new SignalIdSpecification(signalId))
                .FromSpecification(new ActiveSignalSpecification())
                .ToList();

            return result;
        }

        public IReadOnlyList<Signal> GetLatestVersionOfAllSignals()
        {
            var result = BaseQuery()
                .FromSpecification(new ActiveSignalSpecification())
                .GroupBy(r => r.SignalIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return result;
        }

        public IReadOnlyList<Signal> GetLatestVersionOfAllSignals(int controllerTypeId)
        {
            var result = BaseQuery()
                .Where(w => w.ControllerTypeId == controllerTypeId)
                .FromSpecification(new ActiveSignalSpecification())
                .GroupBy(r => r.SignalIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return result;
        }

        public Signal GetLatestVersionOfSignal(string signalId)
        {
            var result = BaseQuery()
                .FromSpecification(new SignalIdSpecification(signalId))
                .FromSpecification(new ActiveSignalSpecification())
                .FirstOrDefault();

            return result;
        }

        public Signal GetLatestVersionOfSignal(string signalId, DateTime startDate)
        {
            var result = BaseQuery()
                .FromSpecification(new SignalIdSpecification(signalId))
                .Where(signal => signal.Start <= startDate)
                .FromSpecification(new ActiveSignalSpecification())
                .FirstOrDefault();

            return result;
        }

        public IReadOnlyList<Signal> GetSignalsBetweenDates(string signalId, DateTime startDate, DateTime endDate)
        {
            var result = BaseQuery()
                .FromSpecification(new SignalIdSpecification(signalId))
                .Where(signal => signal.Start > startDate && signal.Start < endDate)
                .FromSpecification(new ActiveSignalSpecification())
                .ToList();

            return result;
        }

        #endregion
    }
}
