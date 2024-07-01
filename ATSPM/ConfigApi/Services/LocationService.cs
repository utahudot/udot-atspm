using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ConfigApi.Models;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Application.Extensions;
using Google.Api;
using Microsoft.EntityFrameworkCore;

namespace ATSPM.ConfigApi.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository locationRepository;
        private readonly IApproachRepository approachRepository;

        public LocationService(ILocationRepository locationRepository, IApproachRepository approachRepository)
        {
            this.locationRepository = locationRepository;
            this.approachRepository = approachRepository;
        }


        /// <summary>
        /// Copies <see cref="Location"/> and associated <see cref="Approach"/> to new version
        /// and archives old version
        /// </summary>
        /// <param name="id">Location version to copy</param>
        /// <returns>New version of copied <see cref="Location"/></returns>
        public async Task<Location> CopyLocationToNewVersion(int id)
        {
            var sourceLocation = locationRepository.GetLatestVersionOfLocationDetached(id.ToString());
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

                await locationRepository.AddAsync(newVersion);

                return newVersion;
            }
            else
            {
                throw new ArgumentException($"{id} is not a valid Location");
            }
        }

    }

}
