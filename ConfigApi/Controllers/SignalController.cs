using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    [ApiVersion(2.0)]
    public class SignalController : AtspmConfigControllerBase<Signal, int>
    {
        private readonly ISignalRepository _repository;

        public SignalController(ISignalRepository repository) : base(repository)
        {
            _repository = repository;
        }

        [MapToApiVersion(2.0)]
        public override Task<IActionResult> Delete(int key)
        {
            return base.Delete(key);
        }

        [HttpPost]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public IActionResult Promote(int key, [FromBody] ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var title = (string)parameters["title"];
            return NoContent();
        }

        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(typeof(Signal), Status200OK)]
        public IActionResult Test(
        int key,
        ODataQueryOptions<Signal> options,
        CancellationToken ct) =>
        Ok(new Signal() { Id = key });
    }
}
