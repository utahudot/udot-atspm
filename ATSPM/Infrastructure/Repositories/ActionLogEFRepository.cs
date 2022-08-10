using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace ATSPM.Infrasturcture.Repositories
{
    public class ActionLogEFRepository : ATSPMRepositoryEFBase<ActionLog>, IActionLogRepository
    {
        public ActionLogEFRepository(DbContext db, ILogger<ActionLogEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<ActionLog> GetAllByDate(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }
    }
}
