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
    /// Device controller
    /// </summary>
    [ApiVersion(1.0)]
    public class DeviceController : AtspmConfigControllerBase<Device, int>
    {
        private readonly IDeviceRepository _repository;

        /// <inheritdoc/>
        public DeviceController(IDeviceRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        #endregion

        #region Actions

        #endregion

        #region Functions

        /// <summary>
        /// Gets all <see cref="Device"/> from <paramref name="locationId"/> where <see cref="Device.DeviceStatus"/> equals <see cref="DeviceStatus.Active"/>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(IEnumerable<Device>), Status200OK)]
        public IActionResult GetActiveDevicesByLocation(int locationId)
        {
            return Ok(_repository.GetActiveDevicesByLocation(locationId));
        }

        #endregion
    }
}
