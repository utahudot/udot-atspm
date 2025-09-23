//// Services/TestDataSeederService.cs
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.EntityFrameworkCore;
//using Utah.Udot.Atspm.Data;
//using Utah.Udot.Atspm.Data.Enums;
//using Utah.Udot.Atspm.Data.Models;
//using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

//namespace SpeedEventEmitter.Services
//{
//    /// <summary>
//    /// Hosted service to seed test devices, locations, approaches, and detectors at startup.
//    /// Checks for existing devices before seeding.
//    /// </summary>
//    public class TestDataSeederService : IHostedService
//    {
//        private readonly ILocationRepository locationRepository;
//        private readonly IDeviceRepository deviceRepository;
//        private readonly ILogger<TestDataSeederService> _logger;

//        public TestDataSeederService(ILocationRepository locationRepository, IDeviceRepository deviceRepository, ILogger<TestDataSeederService> logger)
//        {
//            this.locationRepository = locationRepository;
//            this.deviceRepository = deviceRepository;
//            _logger = logger;
//        }

//        public async Task StartAsync(CancellationToken cancellationToken)
//        {
          

           

//            _logger.LogInformation("Seeding test data for 50 devices.");



//            // Generate 50 test devices with nested locations, approaches, and detectors
//            var devices = Enumerable.Range(1, 50).Select(i => new Device
//            {
//                // Give each device a unique name
//                DeviceType = DeviceTypes.SpeedSensor,
//                Location =
//                    new Location
//                    {
//                        LocationIdentifier = $"LOC-{i:000}",
//                        Approaches = new[]
//                        {
//                            new Approach
//                            {
//                                Detectors = new[]
//                                {
//                                    new Detector
//                                    {
//                                        DectectorIdentifier = $"DET-{i:000}"
//                                    }
//                                }
//                            }
//                        }
//                    }

//            }).ToList();
//            try
//            {
//                deviceRepository.AddRange(devices);
//                _logger.LogInformation("Seeded {Count} device(s).", devices.Count);
//            }
//            catch (DbUpdateException ex)
//            {
//                _logger.LogError(ex, "Error seeding test data: {Message}", ex.Message);
//                throw;
//            }
//        }

//        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
//    }
//}
