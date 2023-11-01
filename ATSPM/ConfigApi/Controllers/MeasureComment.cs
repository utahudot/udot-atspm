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
    /// Measure comments controller
    /// </summary>
    [ApiVersion(1.0)]
    public class MeasureComment : AtspmConfigControllerBase<Data.Models.MeasureComment, int>
    {
        private readonly IMeasureCommentRepository _repository;

        /// <inheritdoc/>
        public MeasureComment(IMeasureCommentRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        /// <summary>
        /// <see cref="MeasureType"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Expand | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<MeasureType>> GetMeasureTypes([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<MeasureType>>(key);
        }

        #endregion
    }
}
