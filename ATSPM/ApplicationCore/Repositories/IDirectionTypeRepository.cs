using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Direction type repository
    /// </summary>
    public interface IDirectionTypeRepository : IAsyncRepository<DirectionType>
    {
        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<DirectionType> GetAllDirections();

        //[Obsolete("Use Lookup instead")]
        //DirectionType GetDirectionByID(int directionID);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<DirectionType> GetDirectionsByIDs(List<int> includedDirections);

        //[Obsolete("Use GetList instead")]
        //DirectionType GetByDescription(string directionDescription);

        #endregion
    }
}
