using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public class ControllerTypeEFRepository : ATSPMRepositoryEFBase<ControllerType>, IControllerTypeRepository
    {

        public ControllerTypeEFRepository(DbContext db, ILogger<ControllerTypeEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<ControllerType> GetControllerTypes()
        {
            throw new NotImplementedException();
        }
    }
}
