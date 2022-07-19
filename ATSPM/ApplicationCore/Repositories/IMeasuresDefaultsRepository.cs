using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IMeasuresDefaultsRepository : IAsyncRepository<MeasuresDefaults>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<MeasuresDefaults> GetAll();
        IQueryable<string> GetListOfMeasures();
        [Obsolete("Use Lookup instead")]
        IReadOnlyCollection<MeasuresDefaults> GetMeasureDefaults(string chart);
        Dictionary<string, string> GetAllAsDictionary();
        Dictionary<string, string> GetMeasureDefaultsAsDictionary(string chart);
        [Obsolete("Use Update in the BaseClass")]
        void Update(MeasuresDefaults option);
    }
}
