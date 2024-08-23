using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CountyController : SpeedConfigBaseController<NameAndIdDto, Guid>
    {
        public CountyController(ICountyRepository repository) : base(repository)
        {
        }
    }
}
