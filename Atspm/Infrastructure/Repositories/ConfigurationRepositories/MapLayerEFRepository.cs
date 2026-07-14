using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Data;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories
{
    /// <inheritdoc cref="IMapLayerRepository"/>
    public class MapLayerEFRepository : ATSPMRepositoryEFBase<MapLayer>, IMapLayerRepository
    {
        /// <inheritdoc/>
        public MapLayerEFRepository(ConfigContext db, ILogger<MapLayerEFRepository> log) : base(db, log) { }
    }
}
