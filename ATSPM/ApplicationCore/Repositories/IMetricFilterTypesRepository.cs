using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IMetricFilterTypesRepository : IAsyncRepository<MetricsFilterType>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<MetricsFilterType> GetAllFilters();
    }
}
