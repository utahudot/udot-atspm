#region license
// Copyright 2024 Utah Departement of Transportation
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

using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.Atspm.ValueObjects;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ILocationRepository"/>
    /// </summary>
    public static class LocationRepositoryExtensions
    {
        /// <summary>
        /// Copies <see cref="Location"/> and associated <see cref="Approach"/> to new version
        /// and archives old version
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="locationId">Location version to copy</param>
        /// <returns>New version of copied <see cref="Location"/></returns>
        public static async Task<Location> CopyLocationToNewVersion(this ILocationRepository repo, int locationId)
        {
            var sourceLocation = repo.GetVersionByIdDetached(locationId);
            if (sourceLocation != null)
            {
                var newVersion = (Location)sourceLocation.Clone();
                // Detach the original entity

                newVersion.VersionAction = LocationVersionActions.NewVersion;
                newVersion.Start = DateTime.Today;
                newVersion.Note = $"Copy of {sourceLocation.Note}";

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
                foreach (var device in newVersion.Devices)
                {
                    device.Id = 0;
                }

                await repo.AddAsync(newVersion);

                return newVersion;
            }
            else
            {
                throw new ArgumentException($"{locationId} is not a valid Location");
            }
        }

        public static async Task<Location> SaveTemplatedLocation(this ILocationRepository repo, int locationId, TemplateLocationDto replacementInformation)
        {
            var sourceLocation = repo.GetVersionByIdDetached(locationId);
            if (sourceLocation != null)
            {
                var newVersion = (Location)sourceLocation.Clone();
                // Detach the original entity

                newVersion.LocationIdentifier = replacementInformation.LocationIdentifier;
                newVersion.Latitude = replacementInformation.Latitude;
                newVersion.Longitude = replacementInformation.Longitude;
                newVersion.PrimaryName = replacementInformation.PrimaryName;
                newVersion.SecondaryName = replacementInformation.SecondaryName;
                newVersion.VersionAction = LocationVersionActions.New;
                newVersion.Start = DateTime.Today;
                newVersion.Note = replacementInformation.Note == null ? $"Copy of {sourceLocation.Note}" : replacementInformation.Note;

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

                newVersion.Devices = replacementInformation.Devices;

                await repo.AddAsync(newVersion);

                return newVersion;
            }
            else
            {
                throw new ArgumentException($"{locationId} is not a valid Location");
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

        #region Obsolete

        //[Obsolete("This method isn't currently being used")]
        //public static void AddList(this ILocationRepository repo, List<Location> Locations)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use the add in respository base class")]
        //public static void AddOrUpdate(this ILocationRepository repo, Location Location)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("This method isn't currently being used")]
        //public static int CheckVersionWithFirstDate(this ILocationRepository repo, string LocationId)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Redundant to GetAllLocations")]
        //public static IReadOnlyList<Location> EagerLoadAllLocations(this ILocationRepository repo)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Not Required anymore")]
        //public static bool Exists(this ILocationRepository repo, string LocationId)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Redundant to GetLatestVersionOfAllLocations")]
        //public static IReadOnlyList<Location> GetAllEnabledLocations(this ILocationRepository repo)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Redundant to GetLatestVersionOfAllLocations")]
        //public static IList<Location> GetAllLocations(this ILocationRepository repo)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use overload of GetLatestVersionOfAllLocations")]
        //public static IReadOnlyList<Location> GetAllVersionsOfLocationBylocationId(this ILocationRepository repo, string LocationId)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use overload of GetLatestVersionOfAllLocations")]
        //public static IReadOnlyList<Location> GetLatestVerionOfAllLocationsByControllerType(this ILocationRepository repo, int controllerTypeId)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("This method isn't currently being used")]
        //public static string GetLocationDescription(this ILocationRepository repo, string LocationId)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use GetLatestVersionOfLocation")]
        //public static Location GetLatestVersionOfLocationBylocationId(this ILocationRepository repo, string LocationId)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("This should not be in respository")]
        //public static IReadOnlyList<Pin> GetPinInfo(this ILocationRepository repo)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use GetLatestVersionOfLocation")]
        //public static string GetLocationLocation(this ILocationRepository repo, string LocationId)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use Lookup instead")]
        //public static Location GetLocationVersionByVersionId(this ILocationRepository repo, int versionId)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use GetLatestVersionOfLocation")]
        //public static Location GetVersionOfLocationByDate(this ILocationRepository repo, string LocationId, DateTime startDate)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use GetLatestVersionOfLocation")]
        //public static Location GetVersionOfLocationByDateWithDetectionTypes(this ILocationRepository repo, string LocationId, DateTime startDate)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use SetLocationToDeleted")]
        //public static void SetAllVersionsOfALocationToDeleted(this ILocationRepository repo, string id)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use SetLocationToDeleted")]
        //public static void SetVersionToDeleted(this ILocationRepository repo, int versionId)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}
