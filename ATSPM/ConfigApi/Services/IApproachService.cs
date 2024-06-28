using ATSPM.ConfigApi.Models;

namespace ATSPM.ConfigApi.Services
{
    public interface IApproachService
    {
        Task<ApproachDto> UpsertApproachAsync(ApproachDto dto);
    }
}