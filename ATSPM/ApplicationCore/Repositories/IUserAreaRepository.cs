using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IUserAreaRepository
    {
        Task<IEnumerable<UserArea>> GetAllAsync();
        Task<UserArea> GetByIdAsync(string userId, int jurisdictionId);
        Task<IEnumerable<UserArea>> FindAsync(Expression<Func<UserArea, bool>> predicate);
        Task AddAsync(UserArea userArea);
        Task UpdateAsync(UserArea userArea);
        Task RemoveAsync(UserArea userArea);
    }
}