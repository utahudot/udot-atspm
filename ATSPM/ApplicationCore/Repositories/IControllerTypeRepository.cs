using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Location Controller Type Repository
    /// </summary>
    public interface IControllerTypeRepository : IAsyncRepository<ControllerType>
    {
        #region Obsolete

        //[Obsolete("This Method is obsolete, use 'GetList'", true)]
        //List<ControllerType> GetControllerTypes();

        #endregion
    }
}
