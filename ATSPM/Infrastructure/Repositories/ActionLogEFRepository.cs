using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Application.ValueObjects;
using ATSPM.Data;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public class ActionLogEFRepository : ATSPMRepositoryEFBase<ActionLog>, IActionLogRepository
    {
        public ActionLogEFRepository(ConfigContext db, ILogger<ActionLogEFRepository> log) : base(db, log) { }

        #region IActionLogRepository

        #endregion
    }
}
