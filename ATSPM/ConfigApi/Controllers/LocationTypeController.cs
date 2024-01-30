using Asp.Versioning;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;
using ATSPM.Data.Models.ConfigurationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Location type controller
    /// </summary>
    [ApiVersion(1.0)]
    public class LocationTypeController : AtspmConfigControllerBase<LocationType, int>
    {
        private readonly ILocationTypeRepository _repository;

        /// <inheritdoc/>
        public LocationTypeController(ILocationTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        /// <summary>
        /// <see cref="Location"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<Location>> GetLocations([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<Location>>(key);
        }

        #endregion

        #region Actions

        #endregion

        #region Functions

        #endregion
    }
}
