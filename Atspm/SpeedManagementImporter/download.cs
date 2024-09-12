using Microsoft.Extensions.Configuration;
using SpeedManagementImporter.Services.Clearguide;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace SpeedManagementImporter
{
    public class Download : Command
    {
        private ISegmentEntityRepository routeEntityTableRepository;
        private IHourlySpeedRepository hourlySpeedRepository;
        private ITempDataRepository tempDataRepository;
        private IConfigurationRoot configuration;

        public Download(ISegmentEntityRepository routeEntityTableRepository, IHourlySpeedRepository hourlySpeedRepository, ITempDataRepository tempDataRepository, IConfigurationRoot configuration) : base("download", "ATSPMImporter download data")
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
            this.routeEntityTableRepository = routeEntityTableRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.tempDataRepository = tempDataRepository;
            this.configuration = configuration;
        }

        private async Task Execute(DateTime startDate, DateTime endDate, int sourceId, string? filePath)
        {
            try
            {
                IDataDownloader downloader;
                if (sourceId == 3)
                {
                    var uploader = new ClearguideFileUploader(routeEntityTableRepository, tempDataRepository);
                    await uploader.FileUploaderAsync(filePath);
                    downloader = new ClearguideFileDownloaderService(routeEntityTableRepository, hourlySpeedRepository, tempDataRepository);
                }
                else
                {
                    var factory = new ImporterFactory(routeEntityTableRepository, hourlySpeedRepository, tempDataRepository, configuration);
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
