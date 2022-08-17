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
    public class MeasuresDefaultsEFRepository : ATSPMRepositoryEFBase<MeasuresDefaults>, IMeasuresDefaultsRepository
    {
        public MeasuresDefaultsEFRepository(DbContext db, ILogger<MeasuresDefaultsEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<MeasuresDefaults> GetAll()
        {
            return _db.Set<MeasuresDefaults>().ToList();
        }

        public Dictionary<string, string> GetAllAsDictionary()
        {
            var defaults = new Dictionary<string, string>();

            var options = GetAll();

            foreach (var option in options)
            {
                defaults.Add(option.Measure + option.OptionName, option.Value);
            }

            return defaults;
        }

        public IQueryable<string> GetListOfMeasures()
        {
            return _db.Set<MeasuresDefaults>().Select(m => m.Measure).Distinct();
        }

        public IReadOnlyCollection<MeasuresDefaults> GetMeasureDefaults(string chart)
        {
            return _db.Set<MeasuresDefaults>().Where(m => m.Measure == chart).ToList();
        }

        public Dictionary<string, string> GetMeasureDefaultsAsDictionary(string chart)
        {
            var defaults = new Dictionary<string, string>();

            var options = GetMeasureDefaults(chart);

            foreach (var option in options)
            {
                defaults.Add(option.OptionName, option.Value);
            }

            return defaults;
        }
    }
}
