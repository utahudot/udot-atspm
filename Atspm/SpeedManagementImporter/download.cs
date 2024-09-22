using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpeedManagementImporter.Services.Clearguide;
using SpeedManagementImporter.Services.Pems;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace SpeedManagementImporter
{
    public class Download : Command
    {
        private ISegmentEntityRepository segmentEntityRepository;
        private readonly ISegmentRepository segmentRepository;
        private IHourlySpeedRepository hourlySpeedRepository;
        private ITempDataRepository tempDataRepository;
        private IConfigurationRoot configuration;
        private ILogger<PemsDownloaderService> pemsLogger;

        public Download(
            ISegmentEntityRepository segmentEntityRepository,
            ISegmentRepository segmentRepository,
            IHourlySpeedRepository hourlySpeedRepository,
            ITempDataRepository tempDataRepository,
            IConfigurationRoot configuration,
            ILogger<PemsDownloaderService> pemsLogger) : base("download", "ATSPMImporter download data")
        {
            var tempStartDateOption = new Option<DateTime>("--startDate", "start date (mm-dd-yyyy)");
            var tempEndDateOption = new Option<DateTime>("--endDate", "end date (mm-dd-yyyy)");
            var sourceId = new Option<int>("--sourceId", "sourceId");
            var filePath = new Option<string?>("--filePath", "filePath");

            AddOption(tempStartDateOption);
            AddOption(tempEndDateOption);
            AddOption(sourceId);
            if (filePath != null)
            {
                AddOption(filePath);
            }

            this.Handler = CommandHandler.Create<DateTime, DateTime, int, string?>(Execute);
            this.segmentEntityRepository = segmentEntityRepository;
            this.segmentRepository = segmentRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.tempDataRepository = tempDataRepository;
            this.configuration = configuration;
            this.pemsLogger = pemsLogger;
        }

        private async Task Execute(DateTime startDate, DateTime endDate, int sourceId, string? filePath)
        {
            try
            {
                IDataDownloader downloader;
                if (sourceId == 3)
                {
                    var uploader = new ClearguideFileUploader(segmentEntityRepository, tempDataRepository);
                    await uploader.FileUploaderAsync(filePath);
                    downloader = new ClearguideFileDownloaderService(segmentEntityRepository, hourlySpeedRepository, tempDataRepository);
                }
                else
                {
                    var factory = new ImporterFactory(segmentRepository, segmentEntityRepository, hourlySpeedRepository, tempDataRepository, configuration, pemsLogger);
                    downloader = factory.createDownloader(sourceId);
                }

                if (downloader is IFileDataDownloader fileDownloader)
                {
                    await fileDownloader.Download(filePath);
                }
                else
                {
                    await downloader.Download(startDate, endDate);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            };

        }
    }
}
