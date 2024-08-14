using ATSPM.Application.Repositories.SpeedManagementRepositories;
using SpeedManagementImporter.Services.Clearguide;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace SpeedManagementImporter
{
    public class Download : Command
    {
        private ISegmentEntityRepository routeEntityTableRepository;
        private IHourlySpeedRepository hourlySpeedRepository;
        private ITempDataRepository tempDataRepository;

        public Download(ISegmentEntityRepository routeEntityTableRepository, IHourlySpeedRepository hourlySpeedRepository, ITempDataRepository tempDataRepository) : base("download", "ATSPMImporter download data")
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
        }

        private async Task Execute(DateTime startDate, DateTime endDate, int sourceId, string? filePath)
        {
            try
            {
                if (sourceId == 4)
                {
                    var uploader = new ClearguideFileUploader(routeEntityTableRepository, tempDataRepository);
                    await uploader.FileUploaderAsync(filePath);
                }
                else
                {
                    var factory = new ImporterFactory(routeEntityTableRepository, hourlySpeedRepository, tempDataRepository);
                    var downloader = factory.createDownloader(sourceId);

                    if (downloader is IFileDataDownloader fileDownloader)
                    {
                        await fileDownloader.Download(filePath);
                    }
                    else
                    {
                        await downloader.Download(startDate, endDate);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            };

        }
    }
}
