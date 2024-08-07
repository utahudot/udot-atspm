﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Repositories.ConfigurationRepositories/ILocationRepository.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.ConfigurationRepositories
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

        ///// <summary>
        ///// Get latest version of all <see cref="Location"/> and related entities by <see cref="ControllerType"/>
        ///// </summary>
        ///// <param name="controllerTypeId">Index of <see cref="ControllerType"/> to filter</param>
        ///// <returns>List of <see cref="Location"/> with newest start date</returns>
        //IReadOnlyList<Location> GetLatestVersionOfAllLocations(int controllerTypeId);

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

        //HACK: This should not be in the repo, needs to be done a different way

        /// <summary>
        /// Get latest version of <see cref="Location"/> and related entities that match <paramref name="LocationId"/>. 
        /// This allows for cloning and editing of the returned location. 
        /// </summary>
        /// <param name="LocationId"></param>
        /// <returns></returns>
        Location GetVersionByIdDetached(int LocationId);

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
