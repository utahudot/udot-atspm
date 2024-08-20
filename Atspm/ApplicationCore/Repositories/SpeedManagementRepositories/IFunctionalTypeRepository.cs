using ATSPM.Data.Models.SpeedManagement.Common;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
{
    public interface IFunctionalTypeRepository : IAsyncRepository<NameAndIdDto>
    {
    }
}
