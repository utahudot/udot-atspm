using ATSPM.Application.ValueObjects;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Signal Controller Repository
    /// </summary>
    public interface ISignalRepository : IAsyncRepository<Signal>
    {
        /// <summary>
        /// Get all active <see cref="Signal"/> and related entities that match <paramref name="signalIdentifier"/>
        /// </summary>
        /// <param name="signalIdentifier">Signal controller identifier</param>
        /// <returns>List of <see cref="Signal"/> in decescing order of start date</returns>
        IReadOnlyList<Signal> GetAllVersionsOfSignal(string signalIdentifier);

        /// <summary>
        /// Get latest version of all <see cref="Signal"/> and related entities
        /// </summary>
        /// <returns>List of <see cref="Signal"/> with newest start date</returns>
        IReadOnlyList<Signal> GetLatestVersionOfAllSignals();

        /// <summary>
        /// Get latest version of all <see cref="Signal"/> and related entities by <see cref="ControllerType"/>
        /// </summary>
        /// <param name="controllerTypeId">Index of <see cref="ControllerType"/> to filter</param>
        /// <returns>List of <see cref="Signal"/> with newest start date</returns>
        IReadOnlyList<Signal> GetLatestVersionOfAllSignals(int controllerTypeId);


        /// <summary>
        /// Get latest version of <see cref="Signal"/> and related entities that match <paramref name="signalIdentifier"/>
        /// </summary>
        /// <param name="signalIdentifier">Signal controller identifier</param>
        /// <returns>Lastest <see cref="Signal"/> version</returns>
        Signal GetLatestVersionOfSignal(string signalIdentifier);

        /// <summary>
        /// Get latest version of <see cref="Signal"/> and related entities that match <paramref name="signalIdentifier"/>
        /// and begin at or before <paramref name="startDate"/>
        /// </summary>
        /// <param name="signalIdentifier">Signal controller identifier</param>
        /// <param name="startDate">Starting date of Signal controllers</param>
        /// <returns>Lastest <see cref="Signal"/> version</returns>
        Signal GetLatestVersionOfSignal(string signalIdentifier, DateTime startDate);

        /// <summary>
        /// Get all active <see cref="Signal"/> and related entities that match <paramref name="signalIdentifier"/>
        /// and start date is between <paramref name="startDate"/> and <paramref name="endDate"/>
        /// </summary>
        /// <param name="signalIdentifier">Signal controller identifier</param>
        /// <param name="startDate">Date controllers are older than</param>
        /// <param name="endDate">Date controllers are newer than</param>
        /// <returns>List of <see cref="Signal"/> in decescing order of start date</returns>
        IReadOnlyList<Signal> GetSignalsBetweenDates(string signalIdentifier, DateTime startDate, DateTime endDate);


        IReadOnlyList<Signal> GetSignalsForMetricType(int metricTypeId);

        #region ExtensionMethods

        //Signal CopySignalToNewVersion(int id);

        //void SetSignalToDeleted(int id);

        //void SetSignalToDeleted(string signalId);

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
