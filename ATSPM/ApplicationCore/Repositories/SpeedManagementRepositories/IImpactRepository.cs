using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
{
    public interface IImpactRepository : IAsyncRepository<Impact>
    {
        Task<List<Impact>> GetInstancesDetails(List<Guid> impactIds);
        Task<Impact> UpdateImpactAsync(Impact item);
    }
}
