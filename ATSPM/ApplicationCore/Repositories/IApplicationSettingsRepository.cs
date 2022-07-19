using ATSPM.Domain.Services;
using ATSPM.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IApplicationSettingsRepository : IAsyncRepository<ApplicationSetting>
    {
        WatchDogApplicationSettings GetWatchDogSettings();
        GeneralSettings GetGeneralSettings();
        DatabaseArchiveSettings GetDatabaseArchiveSettings();
        [Obsolete("Use Save in the BaseClass")]
        void Save(WatchDogApplicationSettings watchDogApplicationSettings);
        [Obsolete("Use Save in the BaseClass")]
        void Save(GeneralSettings generalSettings);
        [Obsolete("Use Save in the BaseClass")]
        void Save(DatabaseArchiveSettings databaseArchiveSettings);
        int GetRawDataLimit();
    }
}
