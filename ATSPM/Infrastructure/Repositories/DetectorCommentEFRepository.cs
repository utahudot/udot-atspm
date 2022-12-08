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
    public class DetectorCommentEFRepository : ATSPMRepositoryEFBase<DetectorComment>, IDetectorCommentRepository
    {
        public DetectorCommentEFRepository(ConfigContext db, ILogger<DetectorCommentEFRepository> log) : base(db, log) { }

        #region IDetectorCommentRepository

        #endregion
    }
}
