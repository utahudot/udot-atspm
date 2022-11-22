using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.DataApi.Controllers
{
    //[ApiController]
    //[Route("[controller]")]
    public class ApproachesController : ATSPMDataControllerBase<Approach, int>
    {
        private readonly ConfigContext _configContext;
        private readonly IApproachRepository _repository;

        public ApproachesController(ConfigContext configContext, IApproachRepository repository) : base(configContext, repository)
        {
            _configContext = configContext;
            _repository = repository;
        }
    }
}
