using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.NetStandardToolkit.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SpeedConfigBaseController<T, TKey> : ControllerBase where T : AtspmConfigModelBase<TKey>
    {
        private readonly IAsyncRepository<T> _repository;

        public SpeedConfigBaseController(IAsyncRepository<T> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// </summary>
        /// <returns>Action result of type</returns>
        /// <response code="200">Items successfully retrieved.</response>
        // GET /Entity
        [HttpGet()]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual ActionResult<IQueryable<T>> Get()
        {
            return Ok(_repository.GetList());
        }
    }
}
