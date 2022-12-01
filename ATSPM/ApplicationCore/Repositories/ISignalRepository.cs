using ATSPM.Application.ValueObjects;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface ISignalRepository : IAsyncRepository<Signal>
    {
        IReadOnlyList<Signal> GetAllVersionsOfSignal(string SignalId);

        Signal GetLatestVersionOfSignal(string SignalId);

        Signal GetLatestVersionOfSignal(string SignalId, DateTime startDate);

        IReadOnlyList<Signal> GetLatestVersionOfAllSignals();

        IReadOnlyList<Signal> GetSignalsBetweenDates(string SignalId, DateTime startDate, DateTime endDate);

        Task SetSignalToDeleted(int id);

        Task SetSignalToDeleted(string signalId);

        #region ExtensionMethods

        //Signal CopySignalToNewVersion(Signal originalVersion);

        #endregion

        #region Obsolete

        //[Obsolete("This method isn't currently being used")]
        //void AddList(List<Signal> signals);

        //[Obsolete("Use the add in respository base class")]
        //void AddOrUpdate(Signal signal);

        //[Obsolete("This method isn't currently being used")]
        //int CheckVersionWithFirstDate(string SignalId);

        //[Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        //IReadOnlyList<Signal> EagerLoadAllSignals();

        //[Obsolete("Not Required anymore")]
        //bool Exists(string SignalId);

        //[Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        //IReadOnlyList<Signal> GetAllEnabledSignals();

        //[Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        //IList<Signal> GetAllSignals();

        //[Obsolete("Use GetAllVersionsOfSignal")]
        //IReadOnlyList<Signal> GetAllVersionsOfSignalBySignalId(string SignalId);

        //[Obsolete("This method isn't currently being used")]
        //IReadOnlyList<Signal> GetLatestVerionOfAllSignalsByControllerType(int ControllerTypeId);

        //[Obsolete("Just get whole object")]
        //string GetSignalDescription(string SignalId);

        //[Obsolete("Use GetLatestVersionOfSignal")]
        //Signal GetLatestVersionOfSignalBySignalId(string SignalId);

        //[Obsolete("This should not be in respository")]
        //IReadOnlyList<Pin> GetPinInfo();

        //[Obsolete("Use GetLatestVersionOfSignal")]
        //string GetSignalLocation(string SignalId);

        //[Obsolete("Use Lookup instead")]
        //Signal GetSignalVersionByVersionId(int versionId);

        //[Obsolete("Use GetLatestVersionOfSignal")]
        //Signal GetVersionOfSignalByDate(string SignalId, DateTime startDate);

        //[Obsolete("Use GetLatestVersionOfSignal")]
        //Signal GetVersionOfSignalByDateWithDetectionTypes(string SignalId, DateTime startDate);

        //[Obsolete("Use SetSignalToDeleted")]
        //void SetAllVersionsOfASignalToDeleted(string id);

        //[Obsolete("Use SetSignalToDeleted")]
        //void SetVersionToDeleted(int versionId);

        #endregion

        //IReadOnlyList<Signal> GetLatestVersionOfAllSignalsForFtp();
        //SignalFTPInfo GetSignalFTPInfoByID(string SignalId);
        //List<SignalFTPInfo> GetSignalFTPInfoForAllFTPSignals();
    }
}
