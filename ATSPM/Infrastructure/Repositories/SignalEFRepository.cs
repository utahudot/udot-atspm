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

        #region ISignalRepository

        public IReadOnlyList<Signal> GetLatestVersionOfAllSignals()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetAllVersionsOfSignal(string SignalId)
        {
            var result = GetDefaultQuery()
                .FromSpecification(new SignalIdSpecification(SignalId))
                //.Where(signal => signal.SignalId == SignalId)
                .FromSpecification(new ActiveSignalSpecification())
                .ToList();

            return result;
        }

        public Signal GetLatestVersionOfSignal(string SignalId)
        {
            var result = GetDefaultQuery()
                .FromSpecification(new SignalIdSpecification(SignalId))
                //.Where(signal => signal.SignalId == SignalId)
                .FromSpecification(new ActiveSignalSpecification())
                .FirstOrDefault();

            return result;
        }

        public Signal GetLatestVersionOfSignal(string SignalId, DateTime startDate)
        {
            var result = GetDefaultQuery()
                .FromSpecification(new SignalIdSpecification(SignalId))
                //Where(signal => signal.SignalId == SignalId)
                .Where(signal => signal.Start <= startDate)
                .FromSpecification(new ActiveSignalSpecification())
                .FirstOrDefault();

            return result;
        }

        public IReadOnlyList<Signal> GetSignalsBetweenDates(string SignalId, DateTime startDate, DateTime endDate)
        {
            var result = GetDefaultQuery()
                .FromSpecification(new SignalIdSpecification(SignalId))
                //.Where(signal => signal.SignalId == SignalId)
                .Where(signal => signal.Start > startDate && signal.Start < endDate)
                .FromSpecification(new ActiveSignalSpecification())
                .ToList();

            return result;
        }

        public async Task SetSignalToDeleted(int id)
        {
            Signal signal = await LookupAsync(id);

            await DeleteSignal(signal);
        }

        public async Task SetSignalToDeleted(string signalId)
        {
            Signal signal = GetList().FirstOrDefault(f => f.SignalId == signalId);

            await DeleteSignal(signal);
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

        private async Task DeleteSignal(Signal signal)
        {
            if (signal != null)
            {
                signal.VersionActionId = SignaVersionActions.Delete;
                await _db.SaveChangesAsync();
            }
        }
    }
}
