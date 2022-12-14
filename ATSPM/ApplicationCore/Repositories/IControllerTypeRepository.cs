using ATSPM.Data.Models;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Signal Controller Type Repository
    /// </summary>
    public interface IControllerTypeRepository : IAsyncRepository<ControllerType>
    {
        #region Obsolete

        //[Obsolete("This Method is obsolete, use 'GetList'", true)]
        //List<ControllerType> GetControllerTypes();

        #endregion
    }
}
