using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpeedManagementImporter.Services.Atspm;
using SpeedManagementImporter.Services.Pems;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace SpeedManagementImporter
{
    public class ImporterFactory : IImporterFactory
    {
        private ISegmentRepository segmentRepository;
        private ISegmentEntityRepository segmentEntityRepository;
        private IHourlySpeedRepository hourlySpeedRepository;
        private ITempDataRepository tempDataRepository;
        private IConfigurationRoot configuration;
        private ILogger<PemsDownloaderService> pemsLogger;

        public ImporterFactory(
            ISegmentRepository segmentRepository,
            ISegmentEntityRepository segmentEntityRepositdory,
            IHourlySpeedRepository hourlySpeedRepository,
            ITempDataRepository tempDataRepository,
            IConfigurationRoot configuration,
            ILogger<PemsDownloaderService> logger)
        {
            this.segmentRepository = segmentRepository;
            this.segmentEntityRepository = segmentEntityRepositdory;
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.tempDataRepository = tempDataRepository;
            this.configuration = configuration;
            this.pemsLogger = logger;
        }
        public IDataDownloader createDownloader(int sourceId)
        {
            switch (sourceId)
            {
                case 1:
                    var atspmDownloader = new AtspmDownloaderService(segmentEntityRepository, hourlySpeedRepository, configuration);
                    return atspmDownloader;
                case 2:
                    var pemsDownloader = new PemsDownloaderService(segmentRepository, hourlySpeedRepository, configuration, pemsLogger);
                    return pemsDownloader;
                default:
                    return null;
            }
        }
    }
}
