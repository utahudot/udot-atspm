﻿using ATSPM.Data.Models;
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
    /// <summary>
    /// Detector comment entity framework repository
    /// </summary>
    public class DetectorCommentEFRepository : ATSPMRepositoryEFBase<DetectorComment>, IDetectorCommentRepository
    {
        /// <inheritdoc/>
        public DetectorCommentEFRepository(ConfigContext db, ILogger<DetectorCommentEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<DetectorComment> GetList()
        {
            return base.GetList()
                .Include(i => i.Detector);
        }

        #endregion

        #region IDetectorCommentRepository

        #endregion
    }
}