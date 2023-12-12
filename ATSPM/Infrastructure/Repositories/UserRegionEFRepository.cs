using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories
{
    public class UserRegionEFRepository : ATSPMRepositoryEFBase<UserRegion>, IUserRegionRepository
    {
        public UserRegionEFRepository(ConfigContext db, ILogger<UserRegionEFRepository> log) : base(db, log) { }



    }
}
