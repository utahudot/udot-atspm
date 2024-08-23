using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccessCategoryController : SpeedConfigBaseController<NameAndIdDto, Guid>
    {
        public AccessCategoryController(IAccessCategoryRepository repository) : base(repository)
        {
        }
    }
}