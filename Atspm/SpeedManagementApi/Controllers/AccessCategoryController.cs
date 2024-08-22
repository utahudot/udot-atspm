using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using Microsoft.AspNetCore.Mvc;

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
