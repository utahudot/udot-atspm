using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IUserJurisdictionRepository
    {
        Task<IEnumerable<UserJurisdiction>> GetAllAsync();
        Task<UserJurisdiction> GetByIdAsync(string userId, int jurisdictionId);
        Task<IEnumerable<UserJurisdiction>> FindAsync(Expression<Func<UserJurisdiction, bool>> predicate);
        Task AddAsync(UserJurisdiction userJurisdiction);
        Task UpdateAsync(UserJurisdiction userJurisdiction);
        Task RemoveAsync(UserJurisdiction userJurisdiction);
    }
}