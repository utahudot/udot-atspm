using ATSPM.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public class ActionEFRepository : ATSPMRepositoryEFBase<Data.Models.Action>, IActionRepository
    {
        public ActionEFRepository(DbContext db, ILogger<ATSPMRepositoryEFBase<Data.Models.Action>> log) : base(db, log)
        {
        }
    }
}
