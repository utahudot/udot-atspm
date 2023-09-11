using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    public class ActionsController : AtspmConfigControllerBase<Data.Models.Action, ActionTypes>
    {
        private readonly IActionRepository _repository;

        public ActionsController(IActionRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
