using ATSPM.Data.Models;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Application.Repositories
{
    public interface IControllerTypeRepository : IAsyncRepository<ControllerType>
    {
        #region Obsolete

        [Obsolete("This Method is obsolete, use 'GetList'")]
        List<ControllerType> GetControllerTypes();

        #endregion
    }
}
