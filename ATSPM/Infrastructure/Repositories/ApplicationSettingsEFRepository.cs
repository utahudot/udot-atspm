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
    public class ApplicationSettingsEFRepository : ATSPMRepositoryEFBase<ApplicationSetting>, IApplicationSettingsRepository
    {
        public ApplicationSettingsEFRepository(ConfigContext db, ILogger<ApplicationSettingsEFRepository> log) : base(db, log) { }

        public ApplicationSetting GetDatabaseArchiveSettings()
        {
            throw new NotImplementedException();
        }

        public ApplicationSetting GetGeneralSettings()
        {
            throw new NotImplementedException();
        }

        public int GetRawDataLimit()
        {
            throw new NotImplementedException();
        }

        public ApplicationSetting GetWatchDogSettings()
        {
            throw new NotImplementedException();
        }

        #region Overrides

        #endregion

        #region IApplicationSettingsRepository

        #endregion
    }
}