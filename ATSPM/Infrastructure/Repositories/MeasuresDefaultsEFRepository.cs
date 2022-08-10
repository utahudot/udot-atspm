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
    public class MeasuresDefaultsEFRepository : ATSPMRepositoryEFBase<MeasuresDefaults>, IMeasuresDefaultsRepository
    {
        public MeasuresDefaultsEFRepository(DbContext db, ILogger<MeasuresDefaultsEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<MeasuresDefaults> GetAll()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetAllAsDictionary()
        {
            throw new NotImplementedException();
        }

        public IQueryable<string> GetListOfMeasures()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<MeasuresDefaults> GetMeasureDefaults(string chart)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetMeasureDefaultsAsDictionary(string chart)
        {
            throw new NotImplementedException();
        }
    }
}
