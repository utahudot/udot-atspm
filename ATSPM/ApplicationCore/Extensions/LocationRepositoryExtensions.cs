using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class LocationRepositoryExtensions
    {
        /// <summary>
        /// Copies <see cref="Location"/> and associated <see cref="Approach"/> to new version
        /// and archives old version
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="id">Location version to copy</param>
        /// <returns>New version of copied <see cref="Location"/></returns>
        public static async Task<Location> CopyLocationToNewVersion(this ILocationRepository repo, int id)
        {
            Location Location = await repo.LookupAsync(id);

            if (Location != null)
            {
                var newVersion = (Location)Location.Clone();

                newVersion.VersionAction = LocationVersionActions.NewVersion;
                newVersion.Start = DateTime.Today;
                newVersion.Note = $"Copy of {Location.Note}";

                newVersion.Id = 0;

                //newVersion.ControllerType = null;
                newVersion.Jurisdiction = null;
                newVersion.Region = null;

                await repo.AddAsync(newVersion);

                return newVersion;
            }
            else
            {
                throw new ArgumentException($"{id} is not a valid Location");
            }
        }

        /// <summary>
        /// Marks <see cref="Location"/> to deleted
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

        #region Obsolete

        [Obsolete("This method isn't currently being used")]
        public static void AddList(this ILocationRepository repo, List<Location> Locations)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use the add in respository base class")]
        public static void AddOrUpdate(this ILocationRepository repo, Location Location)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method isn't currently being used")]
        public static int CheckVersionWithFirstDate(this ILocationRepository repo, string locationId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Redundant to GetAllLocations")]
        public static IReadOnlyList<Location> EagerLoadAllLocations(this ILocationRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not Required anymore")]
        public static bool Exists(this ILocationRepository repo, string locationId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Redundant to GetLatestVersionOfAllLocations")]
        public static IReadOnlyList<Location> GetAllEnabledLocations(this ILocationRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Redundant to GetLatestVersionOfAllLocations")]
        public static IList<Location> GetAllLocations(this ILocationRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use overload of GetLatestVersionOfAllLocations")]
        public static IReadOnlyList<Location> GetAllVersionsOfLocationBylocationId(this ILocationRepository repo, string locationId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use overload of GetLatestVersionOfAllLocations")]
        public static IReadOnlyList<Location> GetLatestVerionOfAllLocationsByControllerType(this ILocationRepository repo, int controllerTypeId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method isn't currently being used")]
        public static string GetLocationDescription(this ILocationRepository repo, string locationId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetLatestVersionOfLocation")]
        public static Location GetLatestVersionOfLocationBylocationId(this ILocationRepository repo, string locationId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This should not be in respository")]
        public static IReadOnlyList<Pin> GetPinInfo(this ILocationRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetLatestVersionOfLocation")]
        public static string GetLocationLocation(this ILocationRepository repo, string locationId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Lookup instead")]
        public static Location GetLocationVersionByVersionId(this ILocationRepository repo, int versionId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetLatestVersionOfLocation")]
        public static Location GetVersionOfLocationByDate(this ILocationRepository repo, string locationId, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetLatestVersionOfLocation")]
        public static Location GetVersionOfLocationByDateWithDetectionTypes(this ILocationRepository repo, string locationId, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use SetLocationToDeleted")]
        public static void SetAllVersionsOfALocationToDeleted(this ILocationRepository repo, string id)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use SetLocationToDeleted")]
        public static void SetVersionToDeleted(this ILocationRepository repo, int versionId)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
