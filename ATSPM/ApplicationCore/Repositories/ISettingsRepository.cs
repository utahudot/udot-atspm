using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Settings repository
    /// </summary>
    public interface ISettingsRepository : IAsyncRepository<Settings>
    {
    }
}
