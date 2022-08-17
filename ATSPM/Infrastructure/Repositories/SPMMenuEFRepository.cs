using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public class SPMMenuEFRepository : ATSPMRepositoryEFBase<Menu>, ISPMMenuRepository
    {
        public SPMMenuEFRepository(DbContext db, ILogger<SPMMenuEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<Menu> GetMenuItems()
        {
            throw new NotImplementedException();
        }
    }
}
