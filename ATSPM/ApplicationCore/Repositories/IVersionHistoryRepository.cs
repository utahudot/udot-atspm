using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Version history repository
    /// </summary>
    public interface IVersionHistoryRepository : IAsyncRepository<VersionHistory>
    {
    }
}
