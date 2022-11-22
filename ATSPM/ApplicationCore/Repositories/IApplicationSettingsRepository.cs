using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IApplicationSettingsRepository : IAsyncRepository<ApplicationSetting>
    {
        //TODO: Change application settings to use IConfiguration pattern and provider
        
        ApplicationSetting GetWatchDogSettings();
        ApplicationSetting GetGeneralSettings();
        ApplicationSetting GetDatabaseArchiveSettings();
        
        //[Obsolete("Use Save in the BaseClass")]
        //void Save(ApplicationSetting watchDogApplicationSettings);
        
        //[Obsolete("Use Save in the BaseClass")]
        //void Save(ApplicationSetting generalSettings);
        
        //[Obsolete("Use Save in the BaseClass")]
        //void Save(ApplicationSetting databaseArchiveSettings);
        
        int GetRawDataLimit();
    }
}
