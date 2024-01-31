using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.ConfigurationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IUserRegionRepository"/>
    public class UserRegionEFRepository : ATSPMRepositoryEFBase<UserRegion>, IUserRegionRepository
    {
        ///<inheritdoc/>
        public UserRegionEFRepository(ConfigContext db, ILogger<UserRegionEFRepository> log) : base(db, log) { }
    }
}
