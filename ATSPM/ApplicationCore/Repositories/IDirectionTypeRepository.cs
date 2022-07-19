using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ATSPM.Application.Repositories
{
    public interface IDirectionTypeRepository : IAsyncRepository<DetectorEventCountAggregation>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<DirectionType> GetAllDirections();
        [Obsolete("Use Lookup instead")]
        DirectionType GetDirectionByID(int directionID);

        IReadOnlyCollection<SelectListItem> GetSelectList();

        IReadOnlyCollection<DirectionType> GetDirectionsByIDs(List<int> includedDirections);
        DirectionType GetByDescription(string directionDescription);
    }
}
