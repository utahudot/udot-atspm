using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Detector entity framework repository
    /// </summary>
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

        #endregion

        #region IDetectorRepository

        //TODO: this needs to be moved out of this repo
        public IReadOnlyList<Detector> GetDetectorsBySignalIdMovementTypeIdDirectionTypeId(string signalId, DirectionTypes directionType, List<MovementTypes> movementTypeIds)
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
