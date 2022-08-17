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

        public DatabaseArchiveSettings GetDatabaseArchiveSettings()
        {
            return _db.Set<DatabaseArchiveSettings>().FirstOrDefault();
        }

        public GeneralSettings GetGeneralSettings()
        {
            return _db.Set<GeneralSettings>().FirstOrDefault();
        }

        public int GetRawDataLimit()
        {
            GeneralSettings gs = GetGeneralSettings();
            int limit = 0;
            if (gs.RawDataCountLimit != null)
            {
                limit = (int)gs.RawDataCountLimit;
            }
            return limit;
        }

        public WatchDogApplicationSettings GetWatchDogSettings()
        {
            return _db.Set<GeneralSettings>().FirstOrDefault();
        }

        public void Save(WatchDogApplicationSettings watchDogApplicationSettings)
        {
            throw new NotImplementedException();
        }

        public void Save(GeneralSettings generalSettings)
        {
            throw new NotImplementedException();
        }

        public void Save(DatabaseArchiveSettings databaseArchiveSettings)
        {
            throw new NotImplementedException();
        }
    }
}
