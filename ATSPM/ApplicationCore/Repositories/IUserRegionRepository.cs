using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    public interface IUserRegionRepository : IAsyncRepository<UserRegion>
    {
        //Task<IEnumerable<UserRegion>> GetAllAsync();
        //Task<UserRegion> GetByIdAsync(string userId, int regionId);
        //Task<IEnumerable<UserRegion>> FindAsync(Expression<Func<UserRegion, bool>> predicate);
        //Task AddAsync(UserRegion userRegion);
        //Task UpdateAsync(UserRegion userRegion);
        //Task RemoveAsync(UserRegion userRegion);
    }
}