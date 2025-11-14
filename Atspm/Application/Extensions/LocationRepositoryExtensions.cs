#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/LocationRepositoryExtensions.cs
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

using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ILocationRepository"/>
    /// </summary>
    public static class LocationRepositoryExtensions
    {
        //TODO: move this to infrastructure because it uses entity framework
        public static IAsyncEnumerable<Location> GetEventsForAggregation(this ILocationRepository repo, DateTime aggregationDate, EventAggregationQueryOptions queryOptions)
        {
            var query = repo.GetList()
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .Include(i => i.Approaches).ThenInclude(i => i.DirectionType)
                .Include(i => i.Areas)
                .Where(location => location.Start <= aggregationDate)
                .FromSpecification(new ActiveLocationSpecification());

            if (queryOptions.IncludedLocations?.Any() == true)
            {
                query = query.Where(w => queryOptions.IncludedLocations.Contains(w.LocationIdentifier));
            }

            if (queryOptions.ExcludedLocations?.Any() == true)
            {
                query = query.Where(w => !queryOptions.ExcludedLocations.Contains(w.LocationIdentifier));
            }

            if (queryOptions.IncludedAreas?.Any() == true)
            {
                query = query.Where(w => w.Areas.Any(a => queryOptions.IncludedAreas.Contains(a.Name)));
            }

            if (queryOptions.IncludedRegions?.Any() == true)
            {
                query = query.Where(w => queryOptions.IncludedRegions.Contains(w.Region.Description));
            }

            if (queryOptions.IncludedJurisdictions?.Any() == true)
            {
                query = query.Where(w => queryOptions.IncludedJurisdictions.Contains(w.Jurisdiction.Name));
            }

            if (queryOptions.IncludedLocationTypes?.Any() == true)
            {
                query = query.Where(w => queryOptions.IncludedLocationTypes.Contains(w.LocationType.Name));
            }

            var result = query
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .AsQueryable()
                .AsAsyncEnumerable()
                .OrderBy(o => o.LocationIdentifier);

            return result;
        }

        /// <summary>
        /// Copies <see cref="Location"/> and associated <see cref="Approach"/> to new version
        /// and archives old version
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="id">Location version to copy</param>
        /// <param name="newVersionLabel">Label for new version</param>
        /// <returns>New version of copied <see cref="Location"/></returns>
        public static async Task<Location> CopyLocationToNewVersion(this ILocationRepository repo, int id, string newVersionLabel)
        {
            var sourceLocation = repo.GetVersionByIdDetached(id);
            if (sourceLocation != null)
            {
                var newVersion = (Location)sourceLocation.Clone();
                // Detach the original entity

                newVersion.VersionAction = LocationVersionActions.NewVersion;
                newVersion.Start = DateTime.Today;
                newVersion.Note = newVersionLabel;

                newVersion.Id = 0;

                //old devices point to the new objects


                foreach (var approach in newVersion.Approaches)
                {
                    approach.Id = 0;
                    foreach (var detector in approach.Detectors)
                    {
                        detector.Id = 0;
                    }
                }

                newVersion.Devices = null;

                await repo.AddAsync(newVersion);

                return newVersion;
            }
            else
            {
                throw new ArgumentException($"{id} is not a valid Location");
            }
        }

        /// <summary>
        /// Marks <see cref="Location.VersionAction"/> to deleted
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="id">Id of <see cref="Location"/> to mark as deleted</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">id is not a valid Location</exception>
        public static async Task SetLocationToDeleted(this ILocationRepository repo, int id)
        {
            Location Location = await repo.LookupAsync(id);

            if (Location != null)
            {
                Location.VersionAction = LocationVersionActions.Delete;
                await repo.UpdateAsync(Location);
            }
            else
            {
                throw new ArgumentException($"{id} is not a valid Location");
            }
        }

        /// <summary>
        /// Gets the count of each detection type for all active locations as of the specified date.
        /// </summary>
        /// <param name="repo">The location repository.</param>
        /// <param name="date">The date to filter active locations.</param>
        /// <returns>
        /// A list of <see cref="DetectionTypeGroup"/> objects, each containing the detection type description and the count of occurrences across all active locations.
        /// </returns>
        public static List<DetectionTypeGroup> GetDetectionTypeCountForVersions(this ILocationRepository repo, DateTime date)
        {
            var result = repo.GetList()
                .Where(Location => Location.Start <= date)
                .FromSpecification(new ActiveLocationSpecification())
                .SelectMany(location => location.Approaches
                    .SelectMany(approach => approach.Detectors
                        .SelectMany(detector => detector.DetectionTypes
                            .Select(detectionType => new
                            {
                                DetectionTypeDescription = detectionType.Description,
                                LocationStart = location.Start
                            }))))
                .Where(loc => loc.LocationStart <= date)
                .GroupBy(d => d.DetectionTypeDescription)
                .Select(g => new DetectionTypeGroup
                {
                    Id = g.Key,
                    Count = g.Count()
                })
                .ToList();
            return result;
        }
    }
}
