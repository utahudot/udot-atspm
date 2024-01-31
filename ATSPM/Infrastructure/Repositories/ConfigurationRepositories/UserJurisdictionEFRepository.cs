using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.ConfigurationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IUserJurisdictionRepository"/>
    public class UserJurisdictionEFRepository : ATSPMRepositoryEFBase<UserJurisdiction>, IUserJurisdictionRepository
    {
        ///<inheritdoc/>
        public UserJurisdictionEFRepository(ConfigContext db, ILogger<UserJurisdictionEFRepository> log) : base(db, log) { }
    }
}
