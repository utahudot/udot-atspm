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

        public int CheckVersionWithFirstDate(string SignalID)
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

        public bool Exists(string SignalID)
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

        public IReadOnlyList<Signal> GetAllVersionsOfSignalBySignalID(string SignalID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetLatestVerionOfAllSignalsByControllerType(int controllerTypeId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetLatestVersionOfAllSignals()
        {
            throw new NotImplementedException();
        }

        public Signal GetLatestVersionOfSignalBySignalID(string SignalID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Pin> GetPinInfo()
        {
            throw new NotImplementedException();
        }

        public string GetSignalDescription(string SignalID)
        {
            throw new NotImplementedException();
        }

        public string GetSignalLocation(string SignalID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetSignalsBetweenDates(string SignalID, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Signal GetSignalVersionByVersionId(int versionId)
        {
            throw new NotImplementedException();
        }

        public Signal GetVersionOfSignalByDate(string SignalID, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public Signal GetVersionOfSignalByDateWithDetectionTypes(string SignalID, DateTime startDate)
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
