using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FunctionalTypeController : SpeedConfigBaseController<NameAndIdDto, Guid>
    {
        public FunctionalTypeController(IFunctionalTypeRepository repository) : base(repository)
        {
        }
    }
}
