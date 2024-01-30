using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.ConfigurationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    public class UserAreaEFRepository : ATSPMRepositoryEFBase<UserArea>, IUserAreaRepository
    {
        public UserAreaEFRepository(ConfigContext db, ILogger<UserAreaEFRepository> log) : base(db, log) { }
    }
}
