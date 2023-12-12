using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Location controller entity framework repository
    /// </summary>
    public class SignalEFRepository : ATSPMRepositoryEFBase<Location>, ISignalRepository
    {
        /// <inheritdoc/>
        public SignalEFRepository(ConfigContext db, ILogger<SignalEFRepository> log) : base(db, log) { }

        private IQueryable<Location> BaseQuery()
        {
            return base.GetList()
                .Include(i => i.ControllerType)
                .Include(i => i.Jurisdiction)
                .Include(i => i.Region);
                //.Include(i => i.VersionAction);
                //.Include(i => i.Approaches)
                //.ThenInclude(i => i.Detectors)
                //.Include(i => i.Areas);
            //.Include(i => i.MetricComments);
        }

        #region Overrides

        //public override IQueryable<Location> GetList()
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

        /// <inheritdoc/>
        public IReadOnlyList<Location> GetAllVersionsOfSignal(string signalIdentifier)
        {
            var result = BaseQuery()
                .FromSpecification(new SignalIdSpecification(signalIdentifier))
                .FromSpecification(new ActiveSignalSpecification())
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Location> GetLatestVersionOfAllSignals()
        {
            var result = BaseQuery()
                .FromSpecification(new ActiveSignalSpecification())
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Location> GetLatestVersionOfAllSignals(int controllerTypeId)
        {
            var result = BaseQuery()
                .Where(w => w.ControllerTypeId == controllerTypeId)
                .FromSpecification(new ActiveSignalSpecification())
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public Location GetLatestVersionOfSignal(string signalIdentifier)
        {
            var result = BaseQuery()
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .Include(i => i.Approaches).ThenInclude(i => i.DirectionType)
                .Include(i => i.Areas)
                .FromSpecification(new SignalIdSpecification(signalIdentifier))
                .FromSpecification(new ActiveSignalSpecification())
                .FirstOrDefault();

            return result;
        }

        /// <inheritdoc/>
        public Location GetLatestVersionOfSignal(string signalIdentifier, DateTime startDate)
        {
            var result = BaseQuery()
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .Include(i => i.Approaches).ThenInclude(i => i.DirectionType)
                .Include(i => i.Areas)
                .FromSpecification(new SignalIdSpecification(signalIdentifier))
                .Where(signal => signal.Start <= startDate)
                .FromSpecification(new ActiveSignalSpecification())
                .FirstOrDefault();

            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Location> GetLatestVersionOfAllSignals(DateTime startDate)
        {
            var result = BaseQuery()
                 .Include(s => s.Approaches)
                    .ThenInclude(a => a.DirectionType)
                .Include(s => s.Approaches)
                    .ThenInclude(a => a.Detectors)
                .Include(s => s.Approaches)
                    .ThenInclude(a => a.Detectors)
                        .ThenInclude(d => d.DetectorComments)
                .Include(s => s.Approaches)
                    .ThenInclude(a => a.Detectors)
                        .ThenInclude(d => d.DetectionTypes)
                            .ThenInclude(d => d.MeasureTypes)
                .Where(signal => signal.Start <= startDate)
                .FromSpecification(new ActiveSignalSpecification())
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Location> GetSignalsBetweenDates(string signalIdentifier, DateTime startDate, DateTime endDate)
        {
            var result = BaseQuery()
                .FromSpecification(new SignalIdSpecification(signalIdentifier))
                .Where(signal => signal.Start > startDate && signal.Start < endDate)
                .FromSpecification(new ActiveSignalSpecification())
                .ToList();

            var s = new Location();

            s.GetAvailableMetrics();

            return result;
        }

        #endregion
    }
}
