using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Product controller
    /// </summary>
    [ApiVersion(1.0)]
    public class ProductController : AtspmConfigControllerBase<Product, int>
    {
        private readonly IProductRepository _repository;

        /// <inheritdoc/>
        public ProductController(IProductRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        /// <summary>
        /// <see cref="DeviceConfiguration"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Expand | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<Device>> GetDeviceConfigurations([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<Device>>(key);
        }

        #endregion

        #region Actions

        #endregion

        #region Functions

        #endregion
    }
}
