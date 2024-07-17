using SpeedManagementDataDownloader.Business.Common.ImporterFactory;
using SpeedManagementDataDownloader.Common.EntityTable;
using SpeedManagementDataDownloader.Common.HourlySpeeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedManagementDataDownloader
{
    public class Download : Command
    {
        private IRouteEntityTableRepository routeEntityTableRepository;
        private IHourlySpeedRepository hourlySpeedRepository;

        public Download(IRouteEntityTableRepository routeEntityTableRepository, IHourlySpeedRepository hourlySpeedRepository) : base("download", "ATSPMImporter download data")
        {
            var tempStartDateOption = new Option<DateTime>("--startDate", "start date (mm-dd-yyyy)");
            var tempEndDateOption = new Option<DateTime>("--endDate", "end date (mm-dd-yyyy)");
            var sourceId = new Option<int>("--sourceId", "sourceId");

            AddOption(tempStartDateOption);
            AddOption(tempEndDateOption);
            AddOption(sourceId);

            this.Handler = CommandHandler.Create<DateTime, DateTime, int>(Execute);
            this.routeEntityTableRepository = routeEntityTableRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
        }

        private async Task Execute(DateTime startDate, DateTime endDate, int sourceId)
        {
            var factory = new ImporterFactory(routeEntityTableRepository, hourlySpeedRepository);
            var downloader = factory.createDownloader(sourceId);
            try
            {
                await downloader.Download(startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            };
        }
    }
}
