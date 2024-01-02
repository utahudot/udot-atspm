using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// RouteDistance controller
    /// </summary>
    [ApiVersion(1.0)]
    public class RouteDistanceController : AtspmConfigControllerBase<RouteDistance, int>
    {
        private readonly IRouteDistanceRepository _repository;

        /// <inheritdoc/>
        public RouteDistanceController(IRouteDistanceRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        /// <summary>
        /// <see cref="RouteLocation"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<RouteLocation>> GetPreviousLocations([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<RouteLocation>>(key);
        }

        /// <summary>
        /// <see cref="RouteLocation"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<RouteLocation>> GetNextLocations([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<RouteLocation>>(key);
        }

        #endregion

        #region Actions

        #endregion

        #region Functions

        /// <summary>
        /// Gets the <see cref="RouteDistance"/> that contains <paramref name="locationA"/> and <paramref name="locationB"/> 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Select)]
        [ProducesResponseType(typeof(RouteDistance), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        public IActionResult GetRouteDistanceByLocationIdentifiers(string locationA, string locationB)
        {
            return Ok(_repository.GetRouteDistanceByLocationIdentifiers(locationA, locationB));
        }

        #endregion
    }
}
