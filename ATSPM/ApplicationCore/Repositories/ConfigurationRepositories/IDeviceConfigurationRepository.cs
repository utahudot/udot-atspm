using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Route distance repository
    /// </summary>
    public interface IDeviceConfigurationRepository : IAsyncRepository<DeviceConfiguration>
    {
    }

}
