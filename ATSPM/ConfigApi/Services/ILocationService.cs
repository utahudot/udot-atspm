using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Services
{
    public interface ILocationService
    {
        Task<Location> CopyLocationToNewVersion(int id);
    }
}