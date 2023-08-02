using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    [ApiVersion(2.0)]
    public class ActionsController : AtspmConfigControllerBase<Data.Models.Action, int>
    {
        private readonly IActionRepository _repository;

        public ActionsController(IActionRepository repository) : base(repository)
        {
            _repository = repository;
        }

        [MapToApiVersion(2.0)]
        public override Task<IActionResult> Delete(int key)
        {
            return base.Delete(key);
        }
    }
}
