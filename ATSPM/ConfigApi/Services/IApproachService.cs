using ATSPM.ConfigApi.Models;

namespace ATSPM.ConfigApi.Services
{
    public interface IApproachService
    {
        Task<ApproachDto> GetApproachDtoByIdAsync(int id);
        Task<ApproachDto> UpsertApproachAsync(ApproachDto dto);
    }
}