using Asp.Versioning;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Detection type controller
    /// </summary>
    [ApiVersion(1.0)]
    public class DetectionTypeController : AtspmConfigControllerBase<DetectionType, DetectionTypes>
    {
        private readonly IDetectionTypeRepository _repository;

        /// <inheritdoc/>
        public DetectionTypeController(IDetectionTypeRepository repository) : base(repository)
        {
            _repository = repository;
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
            return GetNavigationProperty<IEnumerable<Detector>>((DetectionTypes)key);
        }

        /// <summary>
        /// <see cref="MeasureType"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<MeasureType>> GetMeasureTypes([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<MeasureType>>((DetectionTypes)key);
        }

        #endregion

        #region Actions

        #endregion

        #region Functions

        #endregion
    }
}
