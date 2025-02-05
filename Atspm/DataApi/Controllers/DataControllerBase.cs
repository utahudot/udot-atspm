using Microsoft.AspNetCore.Mvc;

namespace Utah.Udot.ATSPM.DataApi.Controllers
{
    /// <summary>
    /// Base for all Data api controllers
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class DataControllerBase : ControllerBase
    {
    }
}
