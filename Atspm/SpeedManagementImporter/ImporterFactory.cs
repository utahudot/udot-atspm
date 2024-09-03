using Microsoft.Extensions.Configuration;
using SpeedManagementImporter.Services.Atspm;
using SpeedManagementImporter.Services.Clearguide;
using SpeedManagementImporter.Services.Pems;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace SpeedManagementImporter
{
    public class ImporterFactory : IImporterFactory
    {
        private ISegmentEntityRepository segmentEntityRepository;
        private IHourlySpeedRepository hourlySpeedRepository;
        private ITempDataRepository tempDataRepository;
        private IConfigurationRoot configuration;

        public ImporterFactory(ISegmentEntityRepository routeEntityTableRepository, IHourlySpeedRepository hourlySpeedRepository, ITempDataRepository tempDataRepository, IConfigurationRoot configuration)
        {
            this.segmentEntityRepository = routeEntityTableRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.tempDataRepository = tempDataRepository;
            this.configuration = configuration;
        }
        public IDataDownloader createDownloader(int sourceId)
        {
            switch (sourceId)
            {
                case 1:
                    var atspmDownloader = new AtspmDownloaderService(segmentEntityRepository, hourlySpeedRepository, configuration);
                    return atspmDownloader;
                case 3:
                    var clearguideDownloader = new ClearguideFileDownloaderService(segmentEntityRepository, hourlySpeedRepository, tempDataRepository);
                    return clearguideDownloader;
                case 2:
                    var pemsDownloader = new PemsDownloaderService(segmentEntityRepository, hourlySpeedRepository, configuration);
                    return pemsDownloader;
                default:
                    return null;
            }
        }
    }
}
