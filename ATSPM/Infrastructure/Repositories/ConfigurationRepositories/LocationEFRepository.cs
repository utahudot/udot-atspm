using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.ConfigurationModels;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
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
                        var remove = oldItem.Areas.Except(newItem.Areas, new LambdaEqualityComparer<Area>((a1, a2) => a1.Id == a2.Id));
                        var add = newItem.Areas.Except(oldItem.Areas, new LambdaEqualityComparer<Area>((a1, a2) => a1.Id == a2.Id));

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
                .FromSpecification(new LocationIdSpecification(LocationIdentifier))
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

        //public IReadOnlyList<Location> GetLatestVersionOfAllLocations(int controllerTypeId)
        //{
        //    var result = BaseQuery()
        //        .Where(w => w.ControllerTypeId == controllerTypeId)
        //        .FromSpecification(new ActiveLocationSpecification())
        //        .GroupBy(r => r.SignalIdentifier)
        //        .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
        //        .ToList();

        //    return result;
        //}

        /// <inheritdoc/>
        public Location GetLatestVersionOfLocation(string LocationIdentifier)
        {
            var result = BaseQuery()
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .Include(i => i.Approaches).ThenInclude(i => i.DirectionType)
                .Include(i => i.Areas)
                .FromSpecification(new LocationIdSpecification(LocationIdentifier))
                .FromSpecification(new ActiveLocationSpecification())
                .FirstOrDefault();

            return result;
        }

        /// <inheritdoc/>
        public Location GetLatestVersionOfLocation(string LocationIdentifier, DateTime startDate)
        {
            var result = BaseQuery()
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .Include(i => i.Approaches).ThenInclude(i => i.DirectionType)
                .Include(i => i.Areas)
                .FromSpecification(new LocationIdSpecification(LocationIdentifier))
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
                .FromSpecification(new LocationIdSpecification(LocationIdentifier))
                .Where(Location => Location.Start < startDate && Location.Start < endDate)
                .FromSpecification(new ActiveLocationSpecification())
                .ToList();

            var s = new Location();

            s.GetAvailableMetrics();

            return result;
        }

        #endregion
    }
}
