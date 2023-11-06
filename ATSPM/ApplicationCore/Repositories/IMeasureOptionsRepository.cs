using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Measure options repository
    /// </summary>
    public interface IMeasureOptionsRepository : IAsyncRepository<MeasureOption>
    {
        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<MeasureOption> GetAll();

        //[Obsolete("Use GetList instead")]
        //IQueryable<string> GetListOfMeasures();

        //[Obsolete("Use Lookup instead")]
        //IReadOnlyList<MeasureOption> GetMeasureDefaults(string chart);

        //[Obsolete("Use GetList and ToDictionary instead")]
        //Dictionary<string, string> GetAllAsDictionary();

        //[Obsolete("Use GetList and ToDictionary instead")]
        //Dictionary<string, string> GetMeasureDefaultsAsDictionary(string chart);

        //[Obsolete("Use Update in the BaseClass")]
        //void Update(MeasureOption option);

        #endregion
    }
}
