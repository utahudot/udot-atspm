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
    public class MovementTypeEFRepository : ATSPMRepositoryEFBase<MovementType>, IMovementTypeRepository
    {
        public MovementTypeEFRepository(ConfigContext db, ILogger<MovementTypeEFRepository> log) : base(db, log) { }

        #region Overrides

        public override IQueryable<MovementType> GetList()
        {
            return base.GetList()
                .Include(i => i.Detectors);
        }

        #endregion

        #region IMovementTypeRepository

        #endregion
    }
}
