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
    /// Region Repository
    /// </summary>
    public interface IRegionsRepository : IAsyncRepository<Region>
    {
        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Region> GetAllRegions();

        #endregion
    }
}
