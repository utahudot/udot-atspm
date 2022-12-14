using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Metric Filter Type Repository
    /// <see cref="MetricFilterTypes"/>
    /// </summary>
    [Obsolete("This has been changed to an enum")]
    public interface IMetricFilterTypesRepository : IAsyncRepository<MetricsFilterType>
    {
        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<MetricsFilterType> GetAllFilters();

        #endregion
    }
}
