using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Infrasturcture.Repositories
{
    public class SignalEFRepository : ATSPMRepositoryEFBase<Signal> //, ISignalRepository
    {
        public SignalEFRepository(DbContext db, ILogger<SignalEFRepository> log) : base(db, log) { }
        
        //#region ISignalRepository

        //public void AddList(List<Signal> signals)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AddOrUpdate(Signal signal)
        //{
        //    throw new NotImplementedException();
        //}

        //public int CheckVersionWithFirstDate(string signalId)
        //{
        //    throw new NotImplementedException();
        //}

        //public Signal CopySignalToNewVersion(Signal originalVersion)
        //{
        //    throw new NotImplementedException();
        //}

        //public List<Signal> EagerLoadAllSignals()
        //{
        //    throw new NotImplementedException();
        //}

        //public bool Exists(string signalId)
        //{
        //    throw new NotImplementedException();
        //}

        //public List<Signal> GetAllEnabledSignals()
        //{
        //    throw new NotImplementedException();
        //}

        //public List<Signal> GetAllSignals()
        //{
        //    throw new NotImplementedException();
        //}

        //public List<Signal> GetAllVersionsOfSignalBySignalID(string signalID)
        //{
        //    throw new NotImplementedException();
        //}

        //public List<Signal> GetLatestVerionOfAllSignalsByControllerType(int controllerTypeId)
        //{
        //    throw new NotImplementedException();
        //}

        //public List<Signal> GetLatestVersionOfAllSignals()
        //{
        //    throw new NotImplementedException();
        //}

        //public List<Signal> GetLatestVersionOfAllSignalsForFtp()
        //{
        //    throw new NotImplementedException();
        //}

        //public Signal GetLatestVersionOfSignalBySignalID(string signalID)
        //{
        //    throw new NotImplementedException();
        //}

        //public List<Pin> GetPinInfo()
        //{
        //    throw new NotImplementedException();
        //}

        //public string GetSignalDescription(string signalId)
        //{
        //    throw new NotImplementedException();
        //}

        //public string GetSignalLocation(string signalID)
        //{
        //    throw new NotImplementedException();
        //}

        //public List<Signal> GetSignalsBetweenDates(string signalId, DateTime startDate, DateTime endDate)
        //{
        //    throw new NotImplementedException();
        //}

        //public Signal GetSignalVersionByVersionId(int versionId)
        //{
        //    throw new NotImplementedException();
        //}

        //public Signal GetVersionOfSignalByDate(string signalId, DateTime startDate)
        //{
        //    throw new NotImplementedException();
        //}

        //public Signal GetVersionOfSignalByDateWithDetectionTypes(string signalId, DateTime startDate)
        //{
        //    throw new NotImplementedException();
        //}

        //public void SetAllVersionsOfASignalToDeleted(string id)
        //{
        //    throw new NotImplementedException();
        //}

        //public void SetVersionToDeleted(int versionId)
        //{
        //    throw new NotImplementedException();
        //}

        //#endregion
    }
}
