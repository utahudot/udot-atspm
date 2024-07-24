using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Services;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
{
    public interface IImpactRepository : IAsyncRepository<Impact>
    {
        Task<Impact> UpdateImpactAsync(Impact item);
    }
}
