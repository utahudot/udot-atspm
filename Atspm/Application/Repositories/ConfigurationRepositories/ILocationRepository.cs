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
    /// Repository interface for accessing and querying <see cref="Location"/> entities
    /// and their related versions.
    /// </summary>
    public interface ILocationRepository : IAsyncRepository<Location>
    {
        /// <summary>
        /// Retrieves all active versions of a <see cref="Location"/> and related entities
        /// that match the specified <paramref name="locationIdentifier"/>.
        /// </summary>
        /// <param name="locationIdentifier">Unique identifier of the location controller.</param>
        /// <returns>
        /// A read-only list of <see cref="Location"/> records in descending order of start date.
        /// </returns>
        IReadOnlyList<Location> GetAllVersionsOfLocation(string locationIdentifier);

        /// <summary>
        /// Retrieves the latest version of all <see cref="Location"/> entities and their related data.
        /// </summary>
        /// <returns>
        /// A read-only list of <see cref="Location"/> records, each representing the newest version
        /// by start date.
        /// </returns>
        IReadOnlyList<Location> GetLatestVersionOfAllLocations();

        /// <summary>
        /// Retrieves the latest version of all <see cref="Location"/> entities and their related data
        /// that begin at or after the specified <paramref name="startDate"/>.
        /// </summary>
        /// <param name="startDate">The minimum start date used to filter locations.</param>
        /// <returns>
        /// A read-only list of <see cref="Location"/> records with the newest start date
        /// greater than or equal to <paramref name="startDate"/>.
        /// </returns>
        IReadOnlyList<Location> GetLatestVersionOfAllLocations(DateTime startDate);

        /// <summary>
        /// Retrieves the latest version of a <see cref="Location"/> and related entities
        /// that match the specified <paramref name="locationIdentifier"/>.
        /// </summary>
        /// <param name="locationIdentifier">Unique identifier of the location controller.</param>
        /// <returns>
        /// The most recent <see cref="Location"/> version for the given identifier.
        /// </returns>
        Location GetLatestVersionOfLocation(string locationIdentifier);

        /// <summary>
        /// Retrieves the latest version of a <see cref="Location"/> and related entities
        /// that match the specified <paramref name="locationIdentifier"/> and begin at or before
        /// the specified <paramref name="startDate"/>.
        /// </summary>
        /// <param name="locationIdentifier">Unique identifier of the location controller.</param>
        /// <param name="startDate">The maximum start date used to filter locations.</param>
        /// <returns>
        /// The most recent <see cref="Location"/> version that begins on or before <paramref name="startDate"/>.
        /// </returns>
        Location GetLatestVersionOfLocation(string locationIdentifier, DateTime startDate);

        // HACK: This should not be in the repository; consider refactoring to a different approach.

        /// <summary>
        /// Retrieves a detached version of a <see cref="Location"/> and related entities
        /// by its unique identifier. This allows for cloning and editing of the returned entity
        /// without affecting the tracked context.
        /// </summary>
        /// <param name="locationId">Unique database identifier of the location.</param>
        /// <returns>
        /// A detached <see cref="Location"/> entity.
        /// </returns>
        Location GetVersionByIdDetached(int locationId);

        /// <summary>
        /// Retrieves all active versions of a <see cref="Location"/> and related entities
        /// that match the specified <paramref name="locationIdentifier"/> and whose start date
        /// falls between <paramref name="startDate"/> and <paramref name="endDate"/>.
        /// </summary>
        /// <param name="locationIdentifier">Unique identifier of the location controller.</param>
        /// <param name="startDate">Lower bound of the start date filter.</param>
        /// <param name="endDate">Upper bound of the start date filter.</param>
        /// <returns>
        /// A read-only list of <see cref="Location"/> records in descending order of start date.
        /// </returns>
        IReadOnlyList<Location> GetLocationsBetweenDates(string locationIdentifier, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Determines whether a <see cref="Location"/> exists for the specified identifier.
        /// </summary>
        /// <param name="locationIdentifier">Unique identifier of the location controller.</param>
        /// <returns>
        /// A task that resolves to <c>true</c> if the location exists; otherwise <c>false</c>.
        /// </returns>
        Task<bool> LocationExists(string locationIdentifier);

        /// <summary>
        /// Retrieves the latest version of a <see cref="Location"/> and related entities
        /// that match the specified <paramref name="locationIdentifier"/> and begin at or before
        /// the specified <paramref name="startDate"/>, including associated device information.
        /// </summary>
        /// <param name="locationIdentifier">Unique identifier of the location controller.</param>
        /// <param name="startDate">The maximum start date used to filter locations.</param>
        /// <returns>
        /// The most recent <see cref="Location"/> version with device data.
        /// </returns>
        Location GetLatestVersionOfLocationWithDevice(string locationIdentifier, DateTime startDate);

        /// <summary>
        /// Retrieves the latest versions of all <see cref="Location"/> entities
        /// that include detection type information.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Location"/> records with detection type details.
        /// </returns>
        List<Location> GetLatestLocationsWithDetectionTypes();
    }

}