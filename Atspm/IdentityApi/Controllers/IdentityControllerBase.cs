using Microsoft.AspNetCore.Mvc;

namespace Utah.Udot.ATSPM.IdentityApi.Controllers
{
    /// <summary>
    /// Base for all Identity api controllers
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class IdentityControllerBase : ControllerBase
    {
    }
}