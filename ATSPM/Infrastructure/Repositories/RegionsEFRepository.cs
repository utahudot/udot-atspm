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
    public class RegionsEFRepository : ATSPMRepositoryEFBase<Region>, IRegionsRepository
    {
        public RegionsEFRepository(DbContext db, ILogger<RegionsEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<Region> GetAllRegions()
        {
            return _db.Set<Region>().ToList();
        }
    }
}
