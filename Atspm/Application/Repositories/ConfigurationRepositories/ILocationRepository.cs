#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Repositories.ConfigurationRepositories/ILocationRepository.cs
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



        Location GetLatestVersionOfLocationWithDevice(string LocationIdentifier, DateTime startDate);
        List<Location> GetLatestLocationsWithDetectionTypes();
    }
}