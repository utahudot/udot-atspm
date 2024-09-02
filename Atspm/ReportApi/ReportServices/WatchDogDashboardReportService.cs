using Grpc.Net.Client.Configuration;
using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.WatchDogModels;
using Utah.Udot.Atspm.Repositories;

namespace Utah.Udot.ATSPM.ReportApi.ReportServices
{
    public class WatchDogDashboardReportService
    {
        private readonly IWatchDogEventLogRepository watchDogLogEventRepository;
        private readonly IDeviceConfigurationRepository deviceConfigurationRepository;
        private readonly IDeviceRepository deviceRepository;
        private readonly IProductRepository productRepository;
        private readonly IDetectorRepository detectorRepository;


        public WatchDogDashboardReportService(IWatchDogEventLogRepository watchDogLogEventRepository, IDeviceConfigurationRepository deviceConfigurationRepository, IDeviceRepository deviceRepository, IProductRepository productRepository, IDetectorRepository detectorRepository)
        {
            this.watchDogLogEventRepository = watchDogLogEventRepository;
            this.deviceConfigurationRepository = deviceConfigurationRepository;
            this.deviceRepository = deviceRepository;
            this.productRepository = productRepository;
            this.detectorRepository = detectorRepository;
        }

        public List<WatchDogIssueTypeGroup> GetIssueTypeGroups(WatchDogDashboardOptions options)
        {
            var watchdogEvents = watchDogLogEventRepository.GetList().Where(w => w.Timestamp >= options.Start && w.Timestamp <= options.End);
            var products = productRepository.GetList();
            var devices = deviceRepository.GetList();
            var deviceConfiguration = deviceConfigurationRepository.GetList();
            var result = new List<WatchDogIssueTypeGroup>();

            var query = watchdogEvents
                .Join(devices, logEvent => logEvent.LocationId, device => device.LocationId, (logEvent, device) => new { logEvent, device })
                .Join(deviceConfiguration, combined => combined.device.DeviceConfigurationId, deviceConfig => deviceConfig.Id, (combined, deviceConfig) => new { combined.logEvent, combined.device, deviceConfig })
                .Join(products, combined => combined.deviceConfig.ProductId, product => product.Id, (combined, product) => new ControllerEventWithIssueType
                {
                    IssueType = combined.logEvent.IssueType,
                    Model = product.Model,
                    Manufacturer = product.Manufacturer,
                    Firmware = combined.deviceConfig.Firmware
                });


            var issueDictionary = query.GroupBy(g => g.IssueType).ToDictionary(
                group => group.Key,
                group => group);

            foreach (var issue in issueDictionary)
            {
                var val = new WatchDogIssueTypeGroup
                {
                    IssueType = issue.Key,
                    Name = issue.Key.ToString(),
                    Products = issue.Value.GroupBy(g => (g.Manufacturer, g.Firmware, g.Model)).Select(group => new ProductEvent
                    {
                        Manufacturer = group.Key.Manufacturer,
                        Firmware = group.Key.Firmware,
                        Model = group.Key.Model,
                        Counts = group.Count()
                    }).ToList()
                };
                result.Add(val);
            }

            return result;
        }

        public List<WatchDogDetectionTypeGroup> GetDetectionTypeGroups(WatchDogDashboardOptions options)
        {
            var watchdogEvents = watchDogLogEventRepository.GetList().Where(w => w.Timestamp >= options.Start && w.Timestamp <= options.End && w.ComponentType == WatchDogComponentTypes.Detector);
            var detectors = GetDetectorsWithDetectionTypes();
            var result = new List<WatchDogDetectionTypeGroup>();

            var query = watchdogEvents
                .Join(detectors, logEvent => logEvent.ComponentId, detector => detector.Id, (logEvent, detector) => new DetectionTypeEventWithHardware { 
                    DetectionTypeId = detector.DetectionTypes.FirstOrDefault().Id,
                    DetectionTypeName = detector.DetectionTypes.FirstOrDefault().Description,
                    DetectionHardware = detector.DetectionHardware
                });

            var detectionDictionary = query.GroupBy(g => g.DetectionTypeId)
                .ToDictionary(
                group => group.Key,
                group => group);

            foreach (var detectionType in detectionDictionary)
            {
                var val = new WatchDogDetectionTypeGroup
                {
                    DetectionType = detectionType.Key,
                    Name = detectionType.Key.ToString(),
                    Hardware = detectionType.Value.GroupBy(g => g.DetectionHardware).Select(group => new HardwareEvent {
                        Name = group.Key.GetDisplayName(),
                        Counts = group.Count()
                    }).ToList()
                };

                result.Add(val);
            }

            return result;
        }

        private IQueryable<Detector> GetDetectorsWithDetectionTypes()
        {
            return detectorRepository.GetList().Include(d => d.DetectionTypes);
        }
    }
}
