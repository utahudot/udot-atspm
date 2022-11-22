using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.DataApi.Controllers
{
    //[ApiController]
    //[Route("[controller]")]
    public class ControllerTypesController : ATSPMDataControllerBase<ControllerType, int>
    {
        private readonly ConfigContext _configContext;
        private readonly IControllerTypeRepository _repository;

        public ControllerTypesController(ConfigContext configContext, IControllerTypeRepository repository) : base(configContext, repository)
        {
            _configContext = configContext;
            _repository = repository;
        }
    }
}
