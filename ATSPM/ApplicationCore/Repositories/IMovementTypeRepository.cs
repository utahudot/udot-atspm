using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IMovementTypeRepository : IAsyncRepository<MovementType>
    {
        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<MovementType> GetAllMovementTypes();

        //[Obsolete("Use Lookup instead")]
        //MovementType GetMovementTypeByMovementTypeID(int movementTypeID);

        //[Obsolete("Use Update in the BaseClass")]
        //void Update(MovementType movementType);

        //[Obsolete("Use Add in the BaseClass")]
        //void Add(MovementType movementType);

        //[Obsolete("Use Remove in the BaseClass")]
        //void Remove(MovementType movementType);

        #endregion
    }
}
