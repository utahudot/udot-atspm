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
    public class RoutePhaseDirectionEFRepository : ATSPMRepositoryEFBase<RoutePhaseDirection>, IRoutePhaseDirectionRepository
    {
        public RoutePhaseDirectionEFRepository(ConfigContext db, ILogger<RoutePhaseDirectionEFRepository> log) : base(db, log) { }

        #region Overrides

        public override IQueryable<RoutePhaseDirection> GetList()
        {
            return base.GetList()
                .Include(i => i.DirectionType)
                .Include(i => i.RouteSignal);
        }

        #endregion

        #region IRoutePhaseDirectionRepository

        #endregion
    }
}
