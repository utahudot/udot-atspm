using ATSPM.Application.Models;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Repositories
{
    public interface ISignalRepository : IAsyncRepository<Signal>
    {
        IReadOnlyList<Signal> GetAllSignals();
        string GetSignalDescription(string signalId);
        IReadOnlyList<Signal> GetAllEnabledSignals();
        IReadOnlyList<Signal> EagerLoadAllSignals();
        Signal GetLatestVersionOfSignalBySignalID(string signalID);
        //SignalFTPInfo GetSignalFTPInfoByID(string signalID);
        //IReadOnlyList<SignalFTPInfo> GetSignalFTPInfoForAllFTPSignals();
        void AddOrUpdate(Signal signal);
        IReadOnlyList<Pin> GetPinInfo();
        string GetSignalLocation(string signalID);
        void AddIReadOnlyList(IReadOnlyList<Signal> signals);
        Signal CopySignalToNewVersion(Signal originalVersion);
        IReadOnlyList<Signal> GetAllVersionsOfSignalBySignalID(string signalID);
        IReadOnlyList<Signal> GetLatestVersionOfAllSignals();
        IReadOnlyList<Signal> GetLatestVersionOfAllSignalsForFtp();
        int CheckVersionWithFirstDate(string signalId);
        IReadOnlyList<Signal> GetLatestVerionOfAllSignalsByControllerType(int controllerTypeId);
        Signal GetVersionOfSignalByDate(string signalId, DateTime startDate);
        Signal GetSignalVersionByVersionId(int versionId);
        void SetVersionToDeleted(int versionId);
        void SetAllVersionsOfASignalToDeleted(string id);
        IReadOnlyList<Signal> GetSignalsBetweenDates(string signalId, DateTime startDate, DateTime endDate);
        bool Exists(string signalId);
        Signal GetVersionOfSignalByDateWithDetectionTypes(string signalId, DateTime startDate);
    }
}
