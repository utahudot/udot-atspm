using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Location Controller Repository
    /// </summary>
    public interface ILocationRepository : IAsyncRepository<Location>
    {
        /// <summary>
        /// Get all active <see cref="Location"/> and related entities that match <paramref name="signalIdentifier"/>
        /// </summary>
        /// <param name="signalIdentifier">Location controller identifier</param>
        /// <returns>List of <see cref="Location"/> in decescing order of start date</returns>
        IReadOnlyList<Location> GetAllVersionsOfSignal(string signalIdentifier);

        /// <summary>
        /// Get latest version of all <see cref="Location"/> and related entities
        /// </summary>
        /// <returns>List of <see cref="Location"/> with newest start date</returns>
        IReadOnlyList<Location> GetLatestVersionOfAllSignals();

        /// <summary>
        /// Get latest version of all <see cref="Location"/> and related entities by <see cref="ControllerType"/>
        /// </summary>
        /// <param name="controllerTypeId">Index of <see cref="ControllerType"/> to filter</param>
        /// <returns>List of <see cref="Location"/> with newest start date</returns>
        IReadOnlyList<Location> GetLatestVersionOfAllSignals(int controllerTypeId);

        /// <summary>
        /// Get latest version of all <see cref="Location"/> and related entities by <see cref="DateTime"/>
        /// </summary>
        /// <param name="startDate">Locations starting  from <see cref="DateTime"/> to filter</param>
        /// <returns>List of <see cref="Location"/> with newest start date</returns>
        IReadOnlyList<Location> GetLatestVersionOfAllSignals(DateTime startDate);


        /// <summary>
        /// Get latest version of <see cref="Location"/> and related entities that match <paramref name="signalIdentifier"/>
        /// </summary>
        /// <param name="signalIdentifier">Location controller identifier</param>
        /// <returns>Lastest <see cref="Location"/> version</returns>
        Location GetLatestVersionOfSignal(string signalIdentifier);

        /// <summary>
        /// Get latest version of <see cref="Location"/> and related entities that match <paramref name="signalIdentifier"/>
        /// and begin at or before <paramref name="startDate"/>
        /// </summary>
        /// <param name="signalIdentifier">Location controller identifier</param>
        /// <param name="startDate">Starting date of Location controllers</param>
        /// <returns>Lastest <see cref="Location"/> version</returns>
        Location GetLatestVersionOfSignal(string signalIdentifier, DateTime startDate);

        /// <summary>
        /// Get all active <see cref="Location"/> and related entities that match <paramref name="signalIdentifier"/>
        /// and start date is between <paramref name="startDate"/> and <paramref name="endDate"/>
        /// </summary>
        /// <param name="signalIdentifier">Location controller identifier</param>
        /// <param name="startDate">Date controllers are older than</param>
        /// <param name="endDate">Date controllers are newer than</param>
        /// <returns>List of <see cref="Location"/> in decescing order of start date</returns>
        IReadOnlyList<Location> GetSignalsBetweenDates(string signalIdentifier, DateTime startDate, DateTime endDate);


        //ReadOnlyList<Location> GetSignalsForMetricType(int metricTypeId);

        #region ExtensionMethods

        //Location CopySignalToNewVersion(int id);

        //void SetSignalToDeleted(int id);

        //void SetSignalToDeleted(string signalId);

        #endregion

        #region Obsolete

        //[Obsolete("This method isn't currently being used")]
        //void AddList(List<Location> signals);

        //[Obsolete("Use the add in respository base class")]
        //void AddOrUpdate(Location signal);

        //[Obsolete("This method isn't currently being used")]
        //int CheckVersionWithFirstDate(string LocationId);

        //[Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        //IReadOnlyList<Location> EagerLoadAllSignals();

        //[Obsolete("Not Required anymore")]
        //bool Exists(string LocationId);

        //[Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        //IReadOnlyList<Location> GetAllEnabledSignals();

        //[Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        //IList<Location> GetAllSignals();

        //[Obsolete("Use GetAllVersionsOfSignal")]
        //IReadOnlyList<Location> GetAllVersionsOfSignalBySignalId(string LocationId);

        //[Obsolete("This method isn't currently being used")]
        //IReadOnlyList<Location> GetLatestVerionOfAllSignalsByControllerType(int ControllerTypeId);

        //[Obsolete("Just get whole object")]
        //string GetSignalDescription(string LocationId);

        //[Obsolete("Use GetLatestVersionOfSignal")]
        //Location GetLatestVersionOfSignalBySignalId(string LocationId);

        //[Obsolete("This should not be in respository")]
        //IReadOnlyList<Pin> GetPinInfo();

        //[Obsolete("Use GetLatestVersionOfSignal")]
        //string GetSignalLocation(string LocationId);

        //[Obsolete("Use Lookup instead")]
        //Location GetSignalVersionByVersionId(int versionId);

        //[Obsolete("Use GetLatestVersionOfSignal")]
        //Location GetVersionOfSignalByDate(string LocationId, DateTime startDate);

        //[Obsolete("Use GetLatestVersionOfSignal")]
        //Location GetVersionOfSignalByDateWithDetectionTypes(string LocationId, DateTime startDate);

        //[Obsolete("Use SetSignalToDeleted")]
        //void SetAllVersionsOfASignalToDeleted(string id);

        //[Obsolete("Use SetSignalToDeleted")]
        //void SetVersionToDeleted(int versionId);

        #endregion

        //IReadOnlyList<Location> GetLatestVersionOfAllSignalsForFtp();
        //SignalFTPInfo GetSignalFTPInfoByID(string LocationId);
        //List<SignalFTPInfo> GetSignalFTPInfoForAllFTPSignals();
    }
}
