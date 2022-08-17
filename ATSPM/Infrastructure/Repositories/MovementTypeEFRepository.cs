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
    public class MovementTypeEFRepository : ATSPMRepositoryEFBase<MovementType>, IMovementTypeRepository
    {
        public MovementTypeEFRepository(DbContext db, ILogger<MovementTypeEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<MovementType> GetAllMovementTypes()
        {
            throw new NotImplementedException();
        }

        public MovementType GetMovementTypeByMovementTypeID(int movementTypeID)
        {
            throw new NotImplementedException();
        }
    }
}
