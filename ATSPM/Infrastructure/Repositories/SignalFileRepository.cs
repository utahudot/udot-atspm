using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ATSPM.Application.Specifications;
using ATSPM.Domain.Services;
using ATSPM.Domain.Specifications;
using System.IO;
using Microsoft.Extensions.Options;
using ATSPM.Application.Configuration;

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

        public int CheckVersionWithFirstDate(string signalId)
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

        public bool Exists(string signalId)
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

        public IReadOnlyList<Signal> GetAllVersionsOfSignalBySignalID(string signalID)
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

        public Signal GetLatestVersionOfSignalBySignalID(string signalID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Pin> GetPinInfo()
        {
            throw new NotImplementedException();
        }

        public string GetSignalDescription(string signalId)
        {
            throw new NotImplementedException();
        }

        public string GetSignalLocation(string signalID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetSignalsBetweenDates(string signalId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Signal GetSignalVersionByVersionId(int versionId)
        {
            throw new NotImplementedException();
        }

        public Signal GetVersionOfSignalByDate(string signalId, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public Signal GetVersionOfSignalByDateWithDetectionTypes(string signalId, DateTime startDate)
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
