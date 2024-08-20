using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CityController : SpeedConfigBaseController<NameAndIdDto, Guid>
    {
        public CityController(ICityRepository repository) : base(repository)
        {
        }
    }
}
