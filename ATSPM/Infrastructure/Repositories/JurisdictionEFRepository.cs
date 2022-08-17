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
    public class JurisdictionEFRepository : ATSPMRepositoryEFBase<Jurisdiction>, IJurisdictionRepository
    {
        public JurisdictionEFRepository(DbContext db, ILogger<JurisdictionEFRepository> log) : base(db, log)
        {

        }

        public void DeleteByID(int jurisdictionId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Jurisdiction> GetAllJurisdictions()
        {
            return _db.Set<Jurisdiction>().OrderBy(j => j.JurisdictionName).ToList();
        }

        public Jurisdiction GetJurisdictionByID(int jurisdictionId)
        {
            return _db.Set<Jurisdiction>().Find(jurisdictionId);
        }

        public Jurisdiction GetJurisdictionByName(string jurisdictionName)
        {
            return _db.Set<Jurisdiction>().Where(j => j.JurisdictionName == jurisdictionName).FirstOrDefault();
        }
    }
}
