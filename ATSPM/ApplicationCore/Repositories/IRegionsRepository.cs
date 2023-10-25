using ATSPM.Data.Models;
using ATSPM.Domain.Services;

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
