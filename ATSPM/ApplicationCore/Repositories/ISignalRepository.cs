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
        [Obsolete("This method isn't currently being used")]
        void AddList(List<Signal> signals);

        [Obsolete("Use the add in respository base class")]
        void AddOrUpdate(Signal signal);

        [Obsolete("This method isn't currently being used")]
        int CheckVersionWithFirstDate(string signalId);

        [Obsolete("Use ICloneable")]
        Signal CopySignalToNewVersion(Signal originalVersion);

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        IReadOnlyList<Signal> EagerLoadAllSignals();

        bool Exists(string signalId);

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        IReadOnlyList<Signal> GetAllEnabledSignals();

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        IList<Signal> GetAllSignals();

        [Obsolete("Use overload of GetLatestVersionOfAllSignals?")]
        IReadOnlyList<Signal> GetAllVersionsOfSignalBySignalID(string signalID);

        [Obsolete("Use overload of GetLatestVersionOfAllSignals?")]
        IReadOnlyList<Signal> GetLatestVerionOfAllSignalsByControllerType(int controllerTypeId);

        IReadOnlyList<Signal> GetLatestVersionOfAllSignals();

        Signal GetLatestVersionOfSignalBySignalID(string signalID);

        [Obsolete("This should not be in respository")]
        IReadOnlyList<Pin> GetPinInfo();

        [Obsolete("Just get whole object")]
        string GetSignalDescription(string signalId);

        [Obsolete("This should not be in respository")]
        string GetSignalLocation(string signalID);

        IReadOnlyList<Signal> GetSignalsBetweenDates(string signalId, DateTime startDate, DateTime endDate);

        Signal GetSignalVersionByVersionId(int versionId);

        Signal GetVersionOfSignalByDate(string signalId, DateTime startDate);

        Signal GetVersionOfSignalByDateWithDetectionTypes(string signalId, DateTime startDate);

        void SetAllVersionsOfASignalToDeleted(string id);

        void SetVersionToDeleted(int versionId);

        //IReadOnlyList<Signal> GetLatestVersionOfAllSignalsForFtp();
        //SignalFTPInfo GetSignalFTPInfoByID(string signalID);
        //List<SignalFTPInfo> GetSignalFTPInfoForAllFTPSignals();
    }
}
