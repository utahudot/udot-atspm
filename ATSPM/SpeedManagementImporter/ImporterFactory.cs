using ATSPM.Application.Repositories.SpeedManagementRepositories;
using SpeedManagementImporter.Services.Atspm;
using SpeedManagementImporter.Services.Clearguide;
using SpeedManagementImporter.Services.Pems;

namespace SpeedManagementImporter
{
    public class ImporterFactory : IImporterFactory
    {
        private ISegmentEntityRepository segmentEntityRepository;
        private IHourlySpeedRepository hourlySpeedRepository;
        private ITempDataRepository tempDataRepository;

        public ImporterFactory(ISegmentEntityRepository routeEntityTableRepository, IHourlySpeedRepository hourlySpeedRepository, ITempDataRepository tempDataRepository)
        {
            this.segmentEntityRepository = routeEntityTableRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.tempDataRepository = tempDataRepository;
        }
        public IDataDownloader createDownloader(int sourceId)
        {
            switch (sourceId)
            {
                case 1:
                    var atspmDownloader = new AtspmDownloaderService(segmentEntityRepository, hourlySpeedRepository);
                    return atspmDownloader;
                case 3:
                    var clearguideDownloader = new ClearguideFileDownloaderService(segmentEntityRepository, hourlySpeedRepository, tempDataRepository);
                    return clearguideDownloader;
                case 2:
                    var pemsDownloader = new PemsDownloaderService(segmentEntityRepository, hourlySpeedRepository);
                    return pemsDownloader;
                default:
                    return null;
            }
        }
    }
}
