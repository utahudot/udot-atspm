using Asp.Versioning;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// MapLayer Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class MapLayerController(IMapLayerRepository repository) : LocationPolicyControllerBase<MapLayer, int>(repository)
    {
        private readonly IMapLayerRepository _repository = repository;
    }
}
