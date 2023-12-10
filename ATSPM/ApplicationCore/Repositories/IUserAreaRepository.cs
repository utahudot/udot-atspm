using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    public interface IUserAreaRepository : IAsyncRepository<UserArea>
    {
        //Task<IEnumerable<UserArea>> GetAllAsync();
        //Task<UserArea> GetByIdAsync(string userId, int jurisdictionId);
        //Task<IEnumerable<UserArea>> FindAsync(Expression<Func<UserArea, bool>> predicate);
        //Task AddAsync(UserArea userArea);
        //Task UpdateAsync(UserArea userArea);
        //Task RemoveAsync(UserArea userArea);
    }
}