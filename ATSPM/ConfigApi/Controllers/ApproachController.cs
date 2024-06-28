using Asp.Versioning;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ConfigApi.Models;
using ATSPM.ConfigApi.Services;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Approaches Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class ApproachController : AtspmGeneralConfigBase<Approach, int>
    {
        private readonly IApproachRepository _repository;
        private readonly ApproachService approachService;

        /// <inheritdoc/>
        public ApproachController(IApproachRepository repository, ApproachService approachService) : base(repository)
        {
            _repository = repository;
            this.approachService = approachService;
        }

        [Authorize(Policy = "CanEditGeneralConfigurations")]
        public override Task<IActionResult> Post([FromBody] Approach item)
        {
            return base.Post(item);
        }

        [Authorize(Policy = "CanEditGeneralConfigurations")]
        public override Task<IActionResult> Patch(int key, [FromBody] Delta<Approach> item)
        {
            return base.Patch(key, item);
        }

        [Authorize(Policy = "CanDeleteGeneralConfigurations")]
        public override Task<IActionResult> Delete(int key)
        {
            return base.Delete(key);
        }

        #region NavigationProperties

        /// <summary>
        /// <see cref="Detector"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<Detector>> GetDetectors([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<Detector>>(key);
        }

        #endregion

        #region Actions

        #endregion

        #region Functions
        [HttpPost("api/v1/UpsertRoute")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> UpsertRoute([FromBody] ApproachDto approach)
        {
            if (approach == null)
            {
                return BadRequest();
            }

            try
            {
                var routeResult = await approachService.UpsertApproachAsync(approach);
                return Ok(routeResult);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion
    }
}
