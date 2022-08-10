using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public class ApplicationSettingsEFRepository : ATSPMRepositoryEFBase<ApplicationSetting>, IApplicationSettingsRepository
    {
        public ApplicationSettingsEFRepository(DbContext db, ILogger<ApplicationSettingsEFRepository> log) : base(db, log)
        {

        }

        DatabaseArchiveSettings IApplicationSettingsRepository.GetDatabaseArchiveSettings()
        {
            throw new NotImplementedException();
        }

        GeneralSettings IApplicationSettingsRepository.GetGeneralSettings()
        {
            throw new NotImplementedException();
        }
        [Obsolete("This method isn't currently being used")]
        int IApplicationSettingsRepository.GetRawDataLimit()
        {
            throw new NotImplementedException();
        }

        WatchDogApplicationSettings IApplicationSettingsRepository.GetWatchDogSettings()
        {
            throw new NotImplementedException();
        }
        [Obsolete("should be in base class")]
        void IApplicationSettingsRepository.Save(WatchDogApplicationSettings watchDogApplicationSettings)
        {
            throw new NotImplementedException();
        }
        [Obsolete("should be in base class")]
        void IApplicationSettingsRepository.Save(GeneralSettings generalSettings)
        {
            throw new NotImplementedException();
        }
        [Obsolete("should be in base class")]
        void IApplicationSettingsRepository.Save(DatabaseArchiveSettings databaseArchiveSettings)
        {
            throw new NotImplementedException();
        }
    }
}
