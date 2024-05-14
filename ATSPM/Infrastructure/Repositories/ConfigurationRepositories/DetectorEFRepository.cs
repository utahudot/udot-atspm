using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IDetectorRepository"/>
    public class DetectorEFRepository : ATSPMRepositoryEFBase<Detector>, IDetectorRepository
    {
        /// <inheritdoc/>
        public DetectorEFRepository(ConfigContext db, ILogger<DetectorEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<Detector> GetList()
        {
            return base.GetList()
                .Include(i => i.Approach);
            //.Include(i => i.DetectionHardware)
            //.Include(i => i.LaneType)
            //.Include(i => i.MovementType)
            //.Include(i => i.DetectorComments)
            //.Include(i => i.DetectionTypes);
        }

        /// <inheritdoc/>
        protected override void UpdateCollections(Detector oldItem, CollectionEntry oldCollection, Detector newItem, CollectionEntry newCollection)
        {
            switch (oldCollection.Metadata.Name)
            {
                case "DetectionTypes":
                    {
                        var remove = oldItem.DetectionTypes.Except(newItem.DetectionTypes, new ConfigEntityIdComparer<DetectionType, DetectionTypes>());
                        var add = newItem.DetectionTypes.Except(oldItem.DetectionTypes, new ConfigEntityIdComparer<DetectionType, DetectionTypes>());

                        foreach (var r in remove)
                        {
                            oldItem.DetectionTypes.Remove(r);
                        }

                        foreach (var a in add)
                        {
                            oldItem.DetectionTypes.Add(a);
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

        #region IDetectorRepository

        //TODO: this needs to be moved out of this repo
        public IReadOnlyList<Detector> GetDetectorsBylocationIdMovementTypeIdDirectionTypeId(string locationId, DirectionTypes directionType, List<MovementTypes> movementTypeIds)
        {
            return _db.Set<Approach>()
                .Where(a => a.DirectionTypeId == directionType)
                .SelectMany(a => a.Detectors)
                .Where(d => movementTypeIds.Contains(d.MovementType))
                .ToList();
        }

        //TODO: this needs to be moved out of this repo
        public int GetMaximumDetectorChannel(int id)
        {
            return _db.Set<Approach>()
                .Where(a => a.LocationId == id)
                .SelectMany(a => a.Detectors)
                .Max(m => m.DetectorChannel);
        }

        #endregion
    }
}
