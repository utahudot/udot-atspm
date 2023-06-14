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

        #region Overrides

        public override IQueryable<Signal> GetList()
        {
            return base.GetList()
                .Include(s => s.ControllerType)
        .Include(s => s.Jurisdiction)
        .Include(s => s.Region)
        .Include(s => s.VersionAction)
        .Include(s => s.Approaches)
            .ThenInclude(a => a.DirectionType)
        .Include(s => s.Approaches)
            .ThenInclude(a => a.Detectors)
                .ThenInclude(d => d.DetectionHardware)
        .Include(s => s.Approaches)
            .ThenInclude(a => a.Detectors)
                .ThenInclude(d => d.LaneType)
        .Include(s => s.Approaches)
            .ThenInclude(a => a.Detectors)
                .ThenInclude(d => d.MovementType)
        .Include(s => s.Approaches)
            .ThenInclude(a => a.Detectors)
                .ThenInclude(d => d.DetectorComments)
        .Include(s => s.Approaches)
            .ThenInclude(a => a.Detectors)
                .ThenInclude(d => d.DetectionTypes)
        .Include(s => s.Areas);
        }

        #endregion

        #region ISignalRepository

        public IReadOnlyList<Signal> GetAllVersionsOfSignal(string signalId)
        {
            var result = GetDefaultQuery()
                .FromSpecification(new SignalIdSpecification(signalId))
                .FromSpecification(new ActiveSignalSpecification())
                .ToList();

            return result;
        }

        public IReadOnlyList<Signal> GetLatestVersionOfAllSignals()
        {
            var result = GetDefaultQuery()
                .FromSpecification(new ActiveSignalSpecification())
                .GroupBy(r => r.SignalId)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return result;
        }

        public IReadOnlyList<Signal> GetLatestVersionOfAllSignals(int controllerTypeId)
        {
            var result = GetDefaultQuery()
                .Where(w => w.ControllerTypeId == controllerTypeId)
                .FromSpecification(new ActiveSignalSpecification())
                .GroupBy(r => r.SignalId)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return result;
        }

        public Signal GetLatestVersionOfSignal(string signalId)
        {
            var result = GetDefaultQuery()
                .FromSpecification(new SignalIdSpecification(signalId))
                .FromSpecification(new ActiveSignalSpecification())
                .FirstOrDefault();

            return result;
        }

        public Signal GetLatestVersionOfSignal(string signalId, DateTime startDate)
        {
            var result = GetDefaultQuery()
                .FromSpecification(new SignalIdSpecification(signalId))
                .Where(signal => signal.Start <= startDate)
                .FromSpecification(new ActiveSignalSpecification())
                .FirstOrDefault();

            return result;
        }

        public IReadOnlyList<Signal> GetSignalsBetweenDates(string signalId, DateTime startDate, DateTime endDate)
        {
            var result = GetDefaultQuery()
                .FromSpecification(new SignalIdSpecification(signalId))
                .Where(signal => signal.Start > startDate && signal.Start < endDate)
                .FromSpecification(new ActiveSignalSpecification())
                .ToList();

            return result;
        }

        #endregion

        private IQueryable<Signal> GetDefaultQuery()
        {
            var result = GetList()
                .Include(i => i.ControllerType)
                .Include(i => i.Jurisdiction)
                .Include(i => i.Region)
                .Include(i => i.VersionAction)

                .Include(s => s.Approaches)
                .ThenInclude(d => d.Detectors)
                .Include(s => s.Areas);

            return result;
        }
    }
}
