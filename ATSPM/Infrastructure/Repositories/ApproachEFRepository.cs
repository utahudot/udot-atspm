using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    public class ApproachEFRepository : ATSPMRepositoryEFBase<Approach>, IApproachRepository
    {
        public ApproachEFRepository(ConfigContext db, ILogger<ApproachEFRepository> log) : base(db, log) { }

        #region Overrides

        public override IQueryable<Approach> GetList()
        {
            return base.GetList()
                .Include(i => i.DirectionType)
                .Include(i => i.Signal)
                .Include(i => i.Detectors)
                    .ThenInclude(d => d.DetectionTypes)
                        .ThenInclude(dt => dt.MetricTypeMetrics)
                .Include(i => i.Detectors)
                    .ThenInclude(d => d.MovementType);
        }

        #endregion

        #region IApproachRepository

        #endregion
    }
}