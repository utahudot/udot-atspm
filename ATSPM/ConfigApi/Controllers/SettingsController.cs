using Asp.Versioning;
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Settings controller
    /// </summary>
    [ApiVersion(1.0)]
    public class SettingsController : AtspmConfigControllerBase<Settings, int>
    {
        private readonly ISettingsRepository _repository;

        /// <inheritdoc/>
        public SettingsController(ISettingsRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        #endregion

        #region Actions

        #endregion

        #region Functions

        /// <summary>
        /// returns a string value by setting key
        /// </summary>
        /// <param name="setting">Unique setting key</param>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Expand | Select)]
        [ProducesResponseType(typeof(string), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public IActionResult LookupSetting(string setting)
        {
            var result = _repository.LookupSetting(setting);

            if (result == null)
            {
                return NotFound(setting);
            }

            return Ok(result);
        }

        #endregion
    }
}
