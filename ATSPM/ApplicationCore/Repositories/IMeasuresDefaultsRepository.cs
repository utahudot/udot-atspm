using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IMeasuresDefaultsRepository : IAsyncRepository<MeasuresDefault>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyList<MeasuresDefault> GetAll();

        [Obsolete("Use GetList instead")]
        IQueryable<string> GetListOfMeasures();
        
        [Obsolete("Use Lookup instead")]
        IReadOnlyList<MeasuresDefault> GetMeasureDefaults(string chart);

        [Obsolete("Use GetList and ToDictionary instead")]
        Dictionary<string, string> GetAllAsDictionary();

        [Obsolete("Use GetList and ToDictionary instead")]
        Dictionary<string, string> GetMeasureDefaultsAsDictionary(string chart);
        
        [Obsolete("Use Update in the BaseClass")]
        void Update(MeasuresDefault option);
    }
}
