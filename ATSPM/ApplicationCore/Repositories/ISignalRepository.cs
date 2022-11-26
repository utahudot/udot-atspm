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
        int CheckVersionWithFirstDate(string SignalId);

        [Obsolete("Use ICloneable")]
        Signal CopySignalToNewVersion(Signal originalVersion);

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        IReadOnlyList<Signal> EagerLoadAllSignals();

        bool Exists(string SignalId);

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        IReadOnlyList<Signal> GetAllEnabledSignals();

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        IList<Signal> GetAllSignals();

        [Obsolete("Use overload of GetLatestVersionOfAllSignals?")]
        IReadOnlyList<Signal> GetAllVersionsOfSignalBySignalId(string SignalId);

        [Obsolete("Use overload of GetLatestVersionOfAllSignals?")]
        IReadOnlyList<Signal> GetLatestVerionOfAllSignalsByControllerType(int ControllerTypeId);

        IReadOnlyList<Signal> GetLatestVersionOfAllSignals();

        Signal GetLatestVersionOfSignalBySignalId(string SignalId);

        [Obsolete("This should not be in respository")]
        IReadOnlyList<Pin> GetPinInfo();

        [Obsolete("Just get whole object")]
        string GetSignalDescription(string SignalId);

        [Obsolete("This should not be in respository")]
        string GetSignalLocation(string SignalId);

        IReadOnlyList<Signal> GetSignalsBetweenDates(string SignalId, DateTime startDate, DateTime endDate);

        Signal GetSignalVersionByVersionId(int versionId);

        Signal GetVersionOfSignalByDate(string SignalId, DateTime startDate);

        Signal GetVersionOfSignalByDateWithDetectionTypes(string SignalId, DateTime startDate);

        void SetAllVersionsOfASignalToDeleted(string id);

        void SetVersionToDeleted(int versionId);

        //IReadOnlyList<Signal> GetLatestVersionOfAllSignalsForFtp();
        //SignalFTPInfo GetSignalFTPInfoByID(string SignalId);
        //List<SignalFTPInfo> GetSignalFTPInfoForAllFTPSignals();
    }
}
