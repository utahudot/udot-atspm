using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace ATSPM.Infrasturcture.Repositories
{
    public class SignalFileRepository : ATSPMFileRepositoryBase<Signal>, ISignalRepository
    {
        public SignalFileRepository(IFileTranscoder fileTranscoder, IOptions<FileRepositoryConfiguration> options, ILogger<ATSPMFileRepositoryBase<Signal>> log) : base(fileTranscoder, options, log) { }

        public void AddList(List<Signal> signals)
        {
            throw new NotImplementedException();
        }

        public void AddOrUpdate(Signal signal)
        {
            throw new NotImplementedException();
        }

        public int CheckVersionWithFirstDate(string SignalId)
        {
            throw new NotImplementedException();
        }

        public Signal CopySignalToNewVersion(Signal originalVersion)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> EagerLoadAllSignals()
        {
            throw new NotImplementedException();
        }

        public bool Exists(string SignalId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetAllEnabledSignals()
        {
            throw new NotImplementedException();
        }

        public IList<Signal> GetAllSignals()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetAllVersionsOfSignalBySignalId(string SignalId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetLatestVerionOfAllSignalsByControllerType(int ControllerTypeId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetLatestVersionOfAllSignals()
        {
            throw new NotImplementedException();
        }

        public Signal GetLatestVersionOfSignalBySignalId(string SignalId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Pin> GetPinInfo()
        {
            throw new NotImplementedException();
        }

        public string GetSignalDescription(string SignalId)
        {
            throw new NotImplementedException();
        }

        public string GetSignalLocation(string SignalId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetSignalsBetweenDates(string SignalId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Signal GetSignalVersionByVersionId(int versionId)
        {
            throw new NotImplementedException();
        }

        public Signal GetVersionOfSignalByDate(string SignalId, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public Signal GetVersionOfSignalByDateWithDetectionTypes(string SignalId, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public void SetAllVersionsOfASignalToDeleted(string id)
        {
            throw new NotImplementedException();
        }

        public void SetVersionToDeleted(int versionId)
        {
            throw new NotImplementedException();
        }
    }
}
