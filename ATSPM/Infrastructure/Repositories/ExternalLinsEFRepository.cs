﻿using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

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