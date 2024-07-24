using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Services;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementAggregationRepositories
{
    public interface IImpactRepository : IAsyncRepository<Impact>
    {
        Task<Impact> UpdateImpactAsync(Impact item);
    }
}
