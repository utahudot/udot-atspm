using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegionController : SpeedConfigBaseController<NameAndIdDto, Guid>
    {
        public RegionController(IRegionRepository repository) : base(repository)
        {
        }
    }
}
