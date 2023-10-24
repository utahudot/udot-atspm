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
    /// DetectionHardware Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class DetectionHardwareController : AtspmConfigControllerBase<DetectionHardware, DetectionHardwareTypes>
    {
        private readonly IDetectionHardwareRepository _repository;

        /// <inheritdoc/>
        public DetectionHardwareController(IDetectionHardwareRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        /// <summary>
        /// <see cref="Detector"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Expand | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<Detector>> GetDetectors([FromRoute] DetectionHardwareTypes key)
        {
            return GetNavigationProperty<IEnumerable<Detector>>(key);
        }

        #endregion
    }
}
