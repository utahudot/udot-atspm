using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface IAccessCategoryRepository : IAsyncRepository<NameAndIdDto>
    {
    }
}
