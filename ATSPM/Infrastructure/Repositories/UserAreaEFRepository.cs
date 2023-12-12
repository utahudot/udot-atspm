using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories
{
    public class UserAreaEFRepository : ATSPMRepositoryEFBase<UserArea>, IUserAreaRepository
    {
        public UserAreaEFRepository(ConfigContext db, ILogger<UserAreaEFRepository> log) : base(db, log) { }



    }
}
