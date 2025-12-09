#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories/LocationEFRepository.cs
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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Common.EqualityComparers;
using Utah.Udot.Atspm.Data;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="ILocationRepository"/>
    public class LocationEFRepository : ATSPMRepositoryEFBase<Location>, ILocationRepository
    {
        /// <inheritdoc/>
        public LocationEFRepository(ConfigContext db, ILogger<LocationEFRepository> log) : base(db, log) { }

        private IQueryable<Location> BaseQuery()
        {
            return base.GetList()
                .Include(i => i.Jurisdiction)
                .Include(i => i.Region);
            //.Include(i => i.VersionAction);
            //.Include(i => i.Approaches)
            //.ThenInclude(i => i.Detectors)
            //.Include(i => i.Areas);
            //.Include(i => i.MetricComments);
        }

        #region Overrides

        //public override IQueryable<Location> GetList()
        //{
        //    return base.GetList()
        //        .Include(i => i.ControllerType)
        //        .Include(i => i.Jurisdiction)
        //        .Include(i => i.Region)
        //        .Include(i => i.VersionAction)
        //        .Include(i => i.Approaches)
        //        .ThenInclude(i => i.Detectors)
        //        .Include(i => i.Areas);
        //    //.Include(i => i.MetricComments);
        //}

        /// <inheritdoc/>
        protected override void UpdateCollections(Location oldItem, CollectionEntry oldCollection, Location newItem, CollectionEntry newCollection)
        {
            switch (oldCollection.Metadata.Name)
            {
                case "Areas":
                    {
                        var remove = oldItem.Areas.Except(newItem.Areas, new ConfigEntityIdComparer<Area, int>());
                        var add = newItem.Areas.Except(oldItem.Areas, new ConfigEntityIdComparer<Area, int>());

                        foreach (var r in remove)
                        {
                            oldItem.Areas.Remove(r);
                        }

                        foreach (var a in add)
                        {
                            oldItem.Areas.Add(a);
                        }

                        break;
                    }
                default:
                    {
                        base.UpdateCollections(oldItem, oldCollection, newItem, newCollection);

                        break;
                    }
            }
        }

        #endregion

        #region ILocationRepository

        /// <inheritdoc/>
        public IReadOnlyList<Location> GetAllVersionsOfLocation(string LocationIdentifier)
        {
            var result = BaseQuery()
                .FromSpecification(new LocationIdentifierSpecification(LocationIdentifier))
                .FromSpecification(new ActiveLocationSpecification())
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Location> GetLatestVersionOfAllLocations()
        {
            var result = BaseQuery()
                .FromSpecification(new ActiveLocationSpecification())
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public Location GetLatestVersionOfLocation(string LocationIdentifier)
        {
            var result = BaseQuery()
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .Include(i => i.Approaches).ThenInclude(i => i.DirectionType)
                .Include(i => i.Areas)
                .FromSpecification(new LocationIdentifierSpecification(LocationIdentifier))
                .FromSpecification(new ActiveLocationSpecification())
                .FirstOrDefault();

            return result;
        }

        //HACK: This should not be in the repo, needs to be done a different way

        /// <inheritdoc/>
        public Location GetVersionByIdDetached(int LocationId)
        {
            var result = BaseQuery()
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .Include(i => i.Approaches).ThenInclude(i => i.DirectionType)
                .Include(i => i.Devices)
                .Include(i => i.Areas)
                .Where(i => i.Id == LocationId)
                .FromSpecification(new ActiveLocationSpecification())
                .FirstOrDefault();

            _db.Entry(result).State = EntityState.Detached;

            return result;
        }

        /// <inheritdoc/>
        public Location GetLatestVersionOfLocation(string LocationIdentifier, DateTime startDate)
        {
            var result = BaseQuery()
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .Include(i => i.Approaches).ThenInclude(i => i.DirectionType)
                .Include(i => i.Areas)
                .FromSpecification(new LocationIdentifierSpecification(LocationIdentifier))
                .Where(Location => Location.Start <= startDate)
                .FromSpecification(new ActiveLocationSpecification())
                .FirstOrDefault();

            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Location> GetLatestVersionOfAllLocations(DateTime startDate)
        {
            var result = BaseQuery()
                .Include(s => s.Devices)
                .Include(s => s.Approaches)
                    .ThenInclude(a => a.DirectionType)
                .Include(s => s.Approaches)
                    .ThenInclude(a => a.Detectors)
                .Include(s => s.Approaches)
                    .ThenInclude(a => a.Detectors)
                        .ThenInclude(d => d.DetectorComments)
                .Include(s => s.Approaches)
                    .ThenInclude(a => a.Detectors)
                        .ThenInclude(d => d.DetectionTypes)
                            .ThenInclude(d => d.MeasureTypes)
                .Where(Location => Location.Start <= startDate)
                .FromSpecification(new ActiveLocationSpecification())
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Location> GetLocationsBetweenDates(string LocationIdentifier, DateTime startDate, DateTime endDate)
        {
            var result = BaseQuery()
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .FromSpecification(new LocationIdentifierSpecification(LocationIdentifier))
                .Where(Location => Location.Start < startDate && Location.Start < endDate)
                .FromSpecification(new ActiveLocationSpecification())
                .ToList();

            var s = new Location();

            s.GetAvailableMetrics();

            return result;
        }

        /// <inheritdoc/>
        public List<Location> GetLatestLocationsWithDetectionTypes()
        {
            var result = BaseQuery()
                .FromSpecification(new ActiveLocationSpecification())
                .Include(l => l.Approaches)
                    .ThenInclude(a => a.Detectors)
                        .ThenInclude(d => d.DetectionTypes)
                .Include(l => l.Approaches)
                    .ThenInclude(a => a.DirectionType)
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> LocationExists(string locationIdentifier)
        {
            return await GetList().AnyAsync(a => a.LocationIdentifier == locationIdentifier);
        }

        #endregion

        /// <inheritdoc/>
        public Location GetLatestVersionOfLocationWithDevice(string LocationIdentifier, DateTime startDate)
        {
            var result = BaseQuery()
                .Include(l => l.Devices).ThenInclude(d => d.DeviceConfiguration).ThenInclude(d => d.Product)
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .Include(i => i.Approaches).ThenInclude(i => i.DirectionType)
                .Include(i => i.Areas)
                .FromSpecification(new LocationIdentifierSpecification(LocationIdentifier))
                .Where(Location => Location.Start <= startDate)
                .FromSpecification(new ActiveLocationSpecification())
                .FirstOrDefault();

            return result;
        }
    }
}