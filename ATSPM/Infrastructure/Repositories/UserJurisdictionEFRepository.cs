using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories
{
    public class UserJurisdictionEFRepository : ATSPMRepositoryEFBase<UserJurisdiction>, IUserJurisdictionRepository
    {
        public UserJurisdictionEFRepository(ConfigContext db, ILogger<UserJurisdictionEFRepository> log) : base(db, log) { }



    }
}
