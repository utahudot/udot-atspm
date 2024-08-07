#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Services/LocationService.cs
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

using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ConfigApi.Models;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
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
            var sourceLocation = locationRepository.GetVersionByIdDetached(id);
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
