using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Region Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class RegionController : AtspmConfigControllerBase<Region, int>
    {
        private readonly IRegionsRepository _repository;

        /// <inheritdoc/>
        public RegionController(IRegionsRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        [EnableQuery(AllowedQueryOptions = Count | Expand | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<Signal>> GetSignals([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<Signal>>(key);
        }

        #endregion
    }
}
