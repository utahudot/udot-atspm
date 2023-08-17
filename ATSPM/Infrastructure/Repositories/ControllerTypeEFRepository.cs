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
using ATSPM.Domain.Specifications;
using ATSPM.Data;

namespace ATSPM.Infrastructure.Repositories
{
    public class ControllerTypeEFRepository : ATSPMRepositoryEFBase<ControllerType>, IControllerTypeRepository
    {
        public ControllerTypeEFRepository(ConfigContext db, ILogger<ControllerTypeEFRepository> log) : base(db, log) { }

        #region Overrides

        //public override IQueryable<ControllerType> GetList()
        //{
        //    return base.GetList()
        //        .Include(i => i.Signals);
        //}

        #endregion

        #region IControllerEventLogRepository

        #endregion
    }
}
