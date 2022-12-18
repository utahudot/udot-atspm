using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public class SignalFileRepository : ATSPMFileRepositoryBase<Signal>, ISignalRepository
    {
        public SignalFileRepository(IFileTranscoder fileTranscoder, IOptions<FileRepositoryConfiguration> options, ILogger<ATSPMFileRepositoryBase<Signal>> log) : base(fileTranscoder, options, log) { }

        public IReadOnlyList<Signal> GetAllVersionsOfSignal(string SignalId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetLatestVersionOfAllSignals()
        {
            throw new NotImplementedException();
        }

        public Signal GetLatestVersionOfSignal(string SignalId)
        {
            throw new NotImplementedException();
        }

        public Signal GetLatestVersionOfSignal(string SignalId, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetSignalsBetweenDates(string SignalId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task SetSignalToDeleted(int id)
        {
            throw new NotImplementedException();
        }

        public Task SetSignalToDeleted(string signalId)
        {
            throw new NotImplementedException();
        }
    }
}
