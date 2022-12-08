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
    public class JurisdictionEFRepository : ATSPMRepositoryEFBase<Jurisdiction>, IJurisdictionRepository
    {
        public JurisdictionEFRepository(ConfigContext db, ILogger<JurisdictionEFRepository> log) : base(db, log) { }

        #region Overrides

        public override IQueryable<Jurisdiction> GetList()
        {
            return base.GetList().OrderBy(o => o.Name);
        }

        #endregion

        #region IFaqRepository

        #endregion
    }
}
