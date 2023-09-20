using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    public class ApproachController : AtspmConfigControllerBase<Approach, int>
    {
        private readonly IApproachRepository _repository;

        public ApproachController(IApproachRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
