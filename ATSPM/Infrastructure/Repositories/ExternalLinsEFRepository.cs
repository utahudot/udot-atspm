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
    public class ExternalLinsEFRepository : ATSPMRepositoryEFBase<ExternalLink>, IExternalLinksRepository
    {
        public ExternalLinsEFRepository(ConfigContext db, ILogger<ExternalLinsEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region IExternalLinksRepository

        #endregion
    }
}
