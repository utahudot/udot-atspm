using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.ConfigurationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    public class UserJurisdictionEFRepository : ATSPMRepositoryEFBase<UserJurisdiction>, IUserJurisdictionRepository
    {
        public UserJurisdictionEFRepository(ConfigContext db, ILogger<UserJurisdictionEFRepository> log) : base(db, log) { }



    }
}
