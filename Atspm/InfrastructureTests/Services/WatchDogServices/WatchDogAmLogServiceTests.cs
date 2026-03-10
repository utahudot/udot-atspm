using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Xunit;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices.Tests
{
    public class WatchDogAmLogServiceTests
    {
        private readonly Mock<IIndianaEventLogRepository> _controllerEventLogRepositoryMock;
        private readonly AnalysisPhaseCollectionService _analysisPhaseCollectionService;
        private readonly Mock<ILogger<WatchDogRampLogService>> _loggerMock;
        private readonly WatchDogAmLogService _watchDogAmLogService;

        public WatchDogAmLogServiceTests()
        {
            _controllerEventLogRepositoryMock = new Mock<IIndianaEventLogRepository>();
            _loggerMock = new Mock<ILogger<WatchDogRampLogService>>();

            // Real instances for dependencies
            var planService = new PlanService();
            var phaseService = new PhaseService();
            var analysisPhaseService = new AnalysisPhaseService(phaseService);

            _analysisPhaseCollectionService = new AnalysisPhaseCollectionService(
                planService,
                analysisPhaseService
            );

            _watchDogAmLogService = new WatchDogAmLogService(
                _controllerEventLogRepositoryMock.Object,
                _analysisPhaseCollectionService,
                _loggerMock.Object
            );
        }

        [Fact]
        public void Constructor_Should_Create_Instance()
        {
            Assert.NotNull(_watchDogAmLogService);
        }

        [Fact]
        public async Task GetWatchDogIssues_Should_Return_EmptyList_When_LocationsEmpty()
        {
            var options = new WatchdogAmLoggingOptions();
            var result = await _watchDogAmLogService.GetWatchDogIssues(options, new List<Location>(), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWatchDogIssues_Should_HandleCancellation()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var options = new WatchdogAmLoggingOptions();
            var locations = new List<Location> { new Location { LocationIdentifier = "LOC1", Id = 1, Approaches = new List<Approach>() } };

            var result = await _watchDogAmLogService.GetWatchDogIssues(options, locations, cts.Token);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task CheckForStuckPed_Should_AddError_WhenPedestrianCountExceedsThreshold()
        {
            var options = new WatchdogAmLoggingOptions
            {
                AmScanDate = DateTime.Now.Date,
                MaximumPedestrianEvents = 0
            };

            var phase = new AnalysisPhaseData
            {
                PhaseNumber = 1,
                PedestrianEvents = new List<IndianaEvent> { new IndianaEvent { EventCode = 21 } } // 1 ped event
            };

            var approach = new Approach
            {
                Id = 10,
                Location = new Location { Id = 1, LocationIdentifier = "LOC1" }
            };

            var errors = new ConcurrentBag<WatchDogLogEvent>();

            await _watchDogAmLogService.CheckForStuckPed(phase, approach, options, errors);

            Assert.Single(errors);
            Assert.Equal(WatchDogIssueTypes.StuckPed, errors.First().IssueType);
        }

        [Fact]
        public async Task CheckForForceOff_Should_AddError_WhenPercentForceOffExceedsThreshold()
        {
            var options = new WatchdogAmLoggingOptions
            {
                AmScanDate = DateTime.Now.Date,
                PercentThreshold = 0.1,
                MinPhaseTerminations = 0
            };

            var phase = new AnalysisPhaseData
            {
                PhaseNumber = 1,
                PercentForceOffs = 0.5, // exceeds threshold
                TerminationEvents = new List<IndianaEvent> { new IndianaEvent { EventCode = 4 } }
            };

            var approach = new Approach
            {
                Id = 10,
                Location = new Location { Id = 1, LocationIdentifier = "LOC1" }
            };

            var errors = new ConcurrentBag<WatchDogLogEvent>();

            await _watchDogAmLogService.CheckForForceOff(phase, approach, options, errors);

            Assert.Single(errors);
            Assert.Equal(WatchDogIssueTypes.ForceOffThreshold, errors.First().IssueType);
        }

        [Fact]
        public async Task CheckForMaxOut_Should_AddError_WhenPercentMaxOutExceedsThreshold()
        {
            var options = new WatchdogAmLoggingOptions
            {
                AmScanDate = DateTime.Now.Date,
                PercentThreshold = 0.1,
                MinPhaseTerminations = 0
            };

            var phase = new AnalysisPhaseData
            {
                PhaseNumber = 1,
                PercentMaxOuts = 0.5, // exceeds threshold
                TotalPhaseTerminations = 1
            };

            var approach = new Approach
            {
                Id = 10,
                Location = new Location { Id = 1, LocationIdentifier = "LOC1" }
            };

            var errors = new ConcurrentBag<WatchDogLogEvent>();

            await _watchDogAmLogService.CheckForMaxOut(phase, approach, options, errors);

            Assert.Single(errors);
            Assert.Equal(WatchDogIssueTypes.MaxOutThreshold, errors.First().IssueType);
        }


    }
}
