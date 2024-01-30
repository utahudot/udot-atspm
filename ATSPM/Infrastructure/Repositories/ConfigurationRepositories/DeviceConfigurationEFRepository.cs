using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Device configuration entity framework repository
    /// </summary>
    public class DeviceConfigurationEFRepository : ATSPMRepositoryEFBase<DeviceConfiguration>, IDeviceConfigurationRepository
    {
        /// <inheritdoc/>
        public DeviceConfigurationEFRepository(ConfigContext db, ILogger<DeviceConfigurationEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region IDeviceConfigurationRepository

        #endregion
    }
}
