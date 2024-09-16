using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CountyController : SpeedConfigBaseController<NameAndIdDto, Guid>
    {
        public CountyController(ICountyRepository repository) : base(repository)
        {
        }
    }
}
