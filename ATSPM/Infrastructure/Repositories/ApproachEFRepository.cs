using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ATSPM.Application.Specifications;
using ATSPM.Domain.Services;
using ATSPM.Data;

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
                        .ThenInclude(dt => dt.MetricTypeMetrics);
        }

        #endregion

        #region IApproachRepository

        #endregion
    }
}