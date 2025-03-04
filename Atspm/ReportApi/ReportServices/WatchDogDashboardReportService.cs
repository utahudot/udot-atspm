#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.ATSPM.ReportApi.ReportServices/WatchDogDashboardReportService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
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

        public WatchDogDashboardGroup GetDashboardGroup(WatchDogDashboardOptions options)
        {
            var watchdogEvents = watchDogLogEventRepository.GetList().Where(w => w.Timestamp >= options.Start && w.Timestamp <= options.End);
            var watchdogEventsForDetection = watchDogLogEventRepository.GetList().Where(w => w.Timestamp >= options.Start && w.Timestamp <= options.End && w.ComponentType == WatchDogComponentTypes.Detector);
            var products = productRepository.GetList();
            var devices = deviceRepository.GetList();
            var deviceConfiguration = deviceConfigurationRepository.GetList();
            var detectors = GetDetectorsWithDetectionTypes();

            List<WatchDogIssueTypeGroup> issueTypeGroup = GetIssueTypeGroup(watchdogEvents, products, devices, deviceConfiguration);
            List<WatchDogControllerTypeGroup> controllerTypeGroup = GetControllerTypeGroup(watchdogEvents, products, devices, deviceConfiguration);
            List<WatchDogDetectionTypeGroup> detectionTypeGroup = GetDetectionTypeGroup(watchdogEventsForDetection, detectors);

            return new WatchDogDashboardGroup()
            {
                IssueTypeGroup = issueTypeGroup,
                DetectionTypeGroup = detectionTypeGroup,
                ControllerTypeGroup = controllerTypeGroup
            };
        }

        private static List<WatchDogIssueTypeGroup> GetIssueTypeGroup(IQueryable<WatchDogLogEvent> watchdogEvents, IQueryable<Product> products, IQueryable<Device> devices, IQueryable<DeviceConfiguration> deviceConfiguration)
        {
            var issueTypeGroup = new List<WatchDogIssueTypeGroup>();

            var query = GetControllerEventWithIssueType(watchdogEvents, products, devices, deviceConfiguration);

            var issueDictionary = query.GroupBy(g => g.IssueType).ToDictionary(
                group => group.Key,
                group => group);

            foreach (var issue in issueDictionary)
            {
                var val = new WatchDogIssueTypeGroup
                {
                    IssueType = issue.Key,
                    Name = issue.Key.ToString(),
                    Products = issue.Value.GroupBy(g => (g.Manufacturer)).Select(manufacturer => new WatchDogProductInfo
                    {
                        Name = manufacturer.Key,
                        Model = manufacturer.GroupBy(g => g.Model).Select(model => new WatchDogModel<WatchDogFirmwareCount>
                        {
                            Name = model.Key,
                            Firmware = manufacturer.GroupBy(g => g.Firmware).Select(firmware => new WatchDogFirmwareCount
                            {
                                Name = firmware.Key,
                                Counts = firmware.Count()
                            }).ToList()
                        }).ToList()
                    }).ToList()
                };
                issueTypeGroup.Add(val);
            }

            return issueTypeGroup;
        }

        private static List<WatchDogControllerTypeGroup> GetControllerTypeGroup(IQueryable<WatchDogLogEvent> watchdogEvents, IQueryable<Product> products, IQueryable<Device> devices, IQueryable<DeviceConfiguration> deviceConfiguration)
        {
            var controllerTypeGroup = new List<WatchDogControllerTypeGroup>();
            IQueryable<ControllerEventWithIssueType> query = GetControllerEventWithIssueType(watchdogEvents, products, devices, deviceConfiguration);

            var controllerDictionary = query.GroupBy(g => g.Manufacturer).ToDictionary(
                group => group.Key,
                group => group);

            foreach (var controller in controllerDictionary)
            {
                var val = new WatchDogControllerTypeGroup
                {
                    Name = controller.Key.ToString(),
                    Model = controller.Value.GroupBy(g => (g.Model)).Select(manufacturer => new WatchDogModel<WatchDogFirmwareWithIssueType>
                    {
                        Name = manufacturer.Key,
                        Firmware = manufacturer.GroupBy(g => g.Firmware).Select(model => new WatchDogFirmwareWithIssueType
                        {
                            Name = model.Key,
                            IssueType = manufacturer.GroupBy(g => g.IssueType).Select(issueType => new WatchDogIssueTypeCount
                            {
                                Name = issueType.Key.ToString(),
                                Counts = issueType.Count()
                            }).ToList()
                        }).ToList()
                    }).ToList()
                };
                controllerTypeGroup.Add(val);
            }

            return controllerTypeGroup;
        }

        private static IQueryable<ControllerEventWithIssueType> GetControllerEventWithIssueType(IQueryable<WatchDogLogEvent> watchdogEvents, IQueryable<Product> products, IQueryable<Device> devices, IQueryable<DeviceConfiguration> deviceConfiguration)
        {
            return watchdogEvents
                .Join(devices, logEvent => logEvent.LocationId, device => device.LocationId, (logEvent, device) => new { logEvent, device })
                .Join(deviceConfiguration, combined => combined.device.DeviceConfigurationId, deviceConfig => deviceConfig.Id, (combined, deviceConfig) => new { combined.logEvent, combined.device, deviceConfig })
                .Join(products, combined => combined.deviceConfig.ProductId, product => product.Id, (combined, product) => new ControllerEventWithIssueType
                {
                    IssueType = combined.logEvent.IssueType,
                    Model = product.Model,
                    Manufacturer = product.Manufacturer,
                    Firmware = combined.deviceConfig.Description
                });
        }

        public List<WatchDogDetectionTypeGroup> GetDetectionTypeGroups(WatchDogDashboardOptions options)
        {
            var watchdogEvents = watchDogLogEventRepository.GetList().Where(w => w.Timestamp >= options.Start && w.Timestamp <= options.End && w.ComponentType == WatchDogComponentTypes.Detector);
            var detectors = GetDetectorsWithDetectionTypes();
            return GetDetectionTypeGroup(watchdogEvents, detectors);
        }

        private static List<WatchDogDetectionTypeGroup> GetDetectionTypeGroup(IQueryable<WatchDogLogEvent> watchdogEvents, IQueryable<Detector> detectors)
        {
            var result = new List<WatchDogDetectionTypeGroup>();

            var query = watchdogEvents
                .Join(detectors, logEvent => logEvent.ComponentId, detector => detector.Id, (logEvent, detector) => new
                {
                    Detector = detector,
                    LogEvent = logEvent
                })
            .SelectMany(detectorWithEvent => detectorWithEvent.Detector.DetectionTypes.Select(detectionType => new DetectionTypeEventWithHardware
            {
                DetectionTypeId = detectionType.Id,
                DetectionTypeName = detectionType.Description,
                DetectionHardware = detectorWithEvent.Detector.DetectionHardware
            })).ToList();


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
                    Hardware = detectionType.Value.GroupBy(g => g.DetectionHardware).Select(group => new WatchDogHardwareCount
                    {
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
