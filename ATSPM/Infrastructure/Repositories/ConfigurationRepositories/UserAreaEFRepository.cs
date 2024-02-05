using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.ConfigurationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IUserAreaRepository"/>
    public class UserAreaEFRepository : ATSPMRepositoryEFBase<UserArea>, IUserAreaRepository
    {
        ///<inheritdoc/>
        public UserAreaEFRepository(ConfigContext db, ILogger<UserAreaEFRepository> log) : base(db, log) { }
    }
}
