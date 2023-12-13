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
        /// Get all active <see cref="Location"/> and related entities that match <paramref name="LocationIdentifier"/>
        /// </summary>
        /// <param name="LocationIdentifier">Location controller identifier</param>
        /// <returns>List of <see cref="Location"/> in decescing order of start date</returns>
        IReadOnlyList<Location> GetAllVersionsOfLocation(string LocationIdentifier);

        /// <summary>
        /// Get latest version of all <see cref="Location"/> and related entities
        /// </summary>
        /// <returns>List of <see cref="Location"/> with newest start date</returns>
        IReadOnlyList<Location> GetLatestVersionOfAllLocations();

        /// <summary>
        /// Get latest version of all <see cref="Location"/> and related entities by <see cref="ControllerType"/>
        /// </summary>
        /// <param name="controllerTypeId">Index of <see cref="ControllerType"/> to filter</param>
        /// <returns>List of <see cref="Location"/> with newest start date</returns>
        IReadOnlyList<Location> GetLatestVersionOfAllLocations(int controllerTypeId);

        /// <summary>
        /// Get latest version of all <see cref="Location"/> and related entities by <see cref="DateTime"/>
        /// </summary>
        /// <param name="startDate">Locations starting  from <see cref="DateTime"/> to filter</param>
        /// <returns>List of <see cref="Location"/> with newest start date</returns>
        IReadOnlyList<Location> GetLatestVersionOfAllLocations(DateTime startDate);


        /// <summary>
        /// Get latest version of <see cref="Location"/> and related entities that match <paramref name="LocationIdentifier"/>
        /// </summary>
        /// <param name="LocationIdentifier">Location controller identifier</param>
        /// <returns>Lastest <see cref="Location"/> version</returns>
        Location GetLatestVersionOfLocation(string LocationIdentifier);

        /// <summary>
        /// Get latest version of <see cref="Location"/> and related entities that match <paramref name="LocationIdentifier"/>
        /// and begin at or before <paramref name="startDate"/>
        /// </summary>
        /// <param name="LocationIdentifier">Location controller identifier</param>
        /// <param name="startDate">Starting date of Location controllers</param>
        /// <returns>Lastest <see cref="Location"/> version</returns>
        Location GetLatestVersionOfLocation(string LocationIdentifier, DateTime startDate);

        /// <summary>
        /// Get all active <see cref="Location"/> and related entities that match <paramref name="LocationIdentifier"/>
        /// and start date is between <paramref name="startDate"/> and <paramref name="endDate"/>
        /// </summary>
        /// <param name="LocationIdentifier">Location controller identifier</param>
        /// <param name="startDate">Date controllers are older than</param>
        /// <param name="endDate">Date controllers are newer than</param>
        /// <returns>List of <see cref="Location"/> in decescing order of start date</returns>
        IReadOnlyList<Location> GetLocationsBetweenDates(string LocationIdentifier, DateTime startDate, DateTime endDate);


        //ReadOnlyList<Location> GetLocationsForMetricType(int metricTypeId);

        #region ExtensionMethods

        //Location CopyLocationToNewVersion(int id);

        //void SetLocationToDeleted(int id);

        //void SetLocationToDeleted(string LocationId);

        #endregion

        #region Obsolete

        //[Obsolete("This method isn't currently being used")]
        //void AddList(List<Location> Locations);

        //[Obsolete("Use the add in respository base class")]
        //void AddOrUpdate(Location Location);

        //[Obsolete("This method isn't currently being used")]
        //int CheckVersionWithFirstDate(string LocationId);

        //[Obsolete("Redundant to GetLatestVersionOfAllLocations")]
        //IReadOnlyList<Location> EagerLoadAllLocations();

        //[Obsolete("Not Required anymore")]
        //bool Exists(string LocationId);

        //[Obsolete("Redundant to GetLatestVersionOfAllLocations")]
        //IReadOnlyList<Location> GetAllEnabledLocations();

        //[Obsolete("Redundant to GetLatestVersionOfAllLocations")]
        //IList<Location> GetAllLocations();

        //[Obsolete("Use GetAllVersionsOfLocation")]
        //IReadOnlyList<Location> GetAllVersionsOfLocationByLocationId(string LocationId);

        //[Obsolete("This method isn't currently being used")]
        //IReadOnlyList<Location> GetLatestVerionOfAllLocationsByControllerType(int ControllerTypeId);

        //[Obsolete("Just get whole object")]
        //string GetLocationDescription(string LocationId);

        //[Obsolete("Use GetLatestVersionOfLocation")]
        //Location GetLatestVersionOfLocationByLocationId(string LocationId);

        //[Obsolete("This should not be in respository")]
        //IReadOnlyList<Pin> GetPinInfo();

        //[Obsolete("Use GetLatestVersionOfLocation")]
        //string GetLocationLocation(string LocationId);

        //[Obsolete("Use Lookup instead")]
        //Location GetLocationVersionByVersionId(int versionId);

        //[Obsolete("Use GetLatestVersionOfLocation")]
        //Location GetVersionOfLocationByDate(string LocationId, DateTime startDate);

        //[Obsolete("Use GetLatestVersionOfLocation")]
        //Location GetVersionOfLocationByDateWithDetectionTypes(string LocationId, DateTime startDate);

        //[Obsolete("Use SetLocationToDeleted")]
        //void SetAllVersionsOfALocationToDeleted(string id);

        //[Obsolete("Use SetLocationToDeleted")]
        //void SetVersionToDeleted(int versionId);

        #endregion

        //IReadOnlyList<Location> GetLatestVersionOfAllLocationsForFtp();
        //LocationFTPInfo GetLocationFTPInfoByID(string LocationId);
        //List<LocationFTPInfo> GetLocationFTPInfoForAllFTPLocations();
    }
}
