using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IUserRegionRepository
    {
        Task<IEnumerable<UserRegion>> GetAllAsync();
        Task<UserRegion> GetByIdAsync(string userId, int regionId);
        Task<IEnumerable<UserRegion>> FindAsync(Expression<Func<UserRegion, bool>> predicate);
        Task AddAsync(UserRegion userRegion);
        Task UpdateAsync(UserRegion userRegion);
        Task RemoveAsync(UserRegion userRegion);
    }
}