using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Area Repository
    /// </summary>
    public interface IAreaRepository : IAsyncRepository<Area>
    {
        #region ExtensionMethods

        //Area GetAreaByName(string name);

        #endregion

        #region Obsolete

        //[Obsolete("Related collection to Location model")]
        //IReadOnlyList<Area> GetListOfAreasForLocation(string locationId);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Area> GetAllAreas();

        //[Obsolete("Use Lookup instead")]
        //Area GetAreaByID(int areaId);

        //[Obsolete("Use Remove in the BaseClass")]
        //void DeleteByID(int areaId);

        //[Obsolete("Use Remove in the BaseClass")]
        //void Remove(Area Area);

        //[Obsolete("Use Update in the BaseClass")]
        //void Update(Area newArea);

        //[Obsolete("Use Add in the BaseClass")]
        //void Add(Area newArea);

        #endregion
    }
}
