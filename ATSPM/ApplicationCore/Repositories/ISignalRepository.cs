using ATSPM.Application.ValueObjects;
using ATSPM.Data.Models;
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
        int CheckVersionWithFirstDate(string SignalID);

        [Obsolete("Use ICloneable")]
        Signal CopySignalToNewVersion(Signal originalVersion);

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        IReadOnlyList<Signal> EagerLoadAllSignals();

        bool Exists(string SignalID);

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        IReadOnlyList<Signal> GetAllEnabledSignals();

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        IList<Signal> GetAllSignals();

        [Obsolete("Use overload of GetLatestVersionOfAllSignals?")]
        IReadOnlyList<Signal> GetAllVersionsOfSignalBySignalID(string SignalID);

        [Obsolete("Use overload of GetLatestVersionOfAllSignals?")]
        IReadOnlyList<Signal> GetLatestVerionOfAllSignalsByControllerType(int controllerTypeId);

        IReadOnlyList<Signal> GetLatestVersionOfAllSignals();

        Signal GetLatestVersionOfSignalBySignalID(string SignalID);

        [Obsolete("This should not be in respository")]
        IReadOnlyList<Pin> GetPinInfo();

        [Obsolete("Just get whole object")]
        string GetSignalDescription(string SignalID);

        [Obsolete("This should not be in respository")]
        string GetSignalLocation(string SignalID);

        IReadOnlyList<Signal> GetSignalsBetweenDates(string SignalID, DateTime startDate, DateTime endDate);

        Signal GetSignalVersionByVersionId(int versionId);

        Signal GetVersionOfSignalByDate(string SignalID, DateTime startDate);

        Signal GetVersionOfSignalByDateWithDetectionTypes(string SignalID, DateTime startDate);

        void SetAllVersionsOfASignalToDeleted(string id);

        void SetVersionToDeleted(int versionId);

        //IReadOnlyList<Signal> GetLatestVersionOfAllSignalsForFtp();
        //SignalFTPInfo GetSignalFTPInfoByID(string SignalID);
        //List<SignalFTPInfo> GetSignalFTPInfoForAllFTPSignals();
    }
}
