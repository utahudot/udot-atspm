using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ATSPM.Application.Specifications;
using ATSPM.Domain.Services;
using ATSPM.Domain.Specifications;
using ATSPM.Data;

namespace ATSPM.Infrasturcture.Repositories
{
    public class ControllerTypeEFRepository : ATSPMRepositoryEFBase<ControllerType>, IControllerTypeRepository
    {
        public ControllerTypeEFRepository(ConfigContext db, ILogger<ControllerTypeEFRepository> log) : base(db, log) { }

        #region IControllerEventLogRepository

        #region Obsolete

        [Obsolete("This Method is obsolete, use 'GetList'")]
        public List<ControllerType> GetControllerTypes()
        {
            throw new NotImplementedException("This Method is obsolete, use 'GetList'");
        }

        #endregion

        #endregion
    }
}
