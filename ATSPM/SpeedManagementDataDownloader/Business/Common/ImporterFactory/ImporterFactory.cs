using SpeedManagementDataDownloader.Business.Common.DataDownloader;
using SpeedManagementDataDownloader.Business.Services.Clearguide;
using SpeedManagementDataDownloader.Common.EntityTable;
using SpeedManagementDataDownloader.Common.HourlySpeeds;

namespace SpeedManagementDataDownloader.Business.Common.ImporterFactory
{
    public class ImporterFactory : IImporterFactory
    {
        private IRouteEntityTableRepository routeEntityTableRepository;
        private IHourlySpeedRepository hourlySpeedRepository;

        public ImporterFactory(IRouteEntityTableRepository routeEntityTableRepository, IHourlySpeedRepository hourlySpeedRepository)
        {
            this.routeEntityTableRepository = routeEntityTableRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
        }
        public IDataDownloader createDownloader(int sourceId)
        {
            switch (sourceId)
            {
                case 1:
                    var atspmDownloader = new AtspmDownloaderService(routeEntityTableRepository, hourlySpeedRepository);
                    return atspmDownloader;
                case 3:
                    var clearguideDownloader = new ClearguideDownloaderService(routeEntityTableRepository, hourlySpeedRepository);
                    return clearguideDownloader;
                case 2:
                    var pemsDownloader = new PemsDownloaderService(routeEntityTableRepository, hourlySpeedRepository);
                    return pemsDownloader;
                default:
                    return null;
            }
        }
    }
}
