using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IDirectionTypeRepository : IAsyncRepository<DetectorEventCountAggregation>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyList<DirectionType> GetAllDirections();
        
        [Obsolete("Use Lookup instead")]
        DirectionType GetDirectionByID(int directionID);

        [Obsolete("Use GetList instead")]
        IReadOnlyList<DirectionType> GetDirectionsByIDs(List<int> includedDirections);

        [Obsolete("Use GetList instead")]
        DirectionType GetByDescription(string directionDescription);
    }
}
