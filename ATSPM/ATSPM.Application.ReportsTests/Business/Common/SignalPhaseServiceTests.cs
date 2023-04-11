using Microsoft.VisualStudio.TestTools.UnitTesting;
using ATSPM.Application.Reports.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATSPM.Application.Reports.Business.PedDelay;
using Microsoft.Extensions.Logging.Abstractions;
using AutoFixture;


namespace ATSPM.Application.Reports.Business.Common.Tests
{
    [TestClass()]
    public class SignalPhaseServiceTests
    {
        private readonly PlanService planService = new PlanService(null,null);
        private readonly CycleService cycleService = new CycleService(null);
        private readonly NullLogger<SignalPhaseService> logger = new NullLogger<SignalPhaseService>();
        private Fixture fixture;

        [TestInitialize]
        public void TestInitialize()
        {
            fixture = new Fixture();
        }

        [TestMethod]
        public void LinkPivotAddSeconds_ShouldAddSecondsToCycleDetectorEvents()
        {
            // Arrange
            var signalPhase = fixture.Create<SignalPhase>();
            var secondsToAdd = fixture.Create<int>();

            var cycles = fixture.CreateMany<CyclePcd>().ToList();
            signalPhase.Cycles.AddRange(cycles);

            // Act
            var signalPhaseService = new SignalPhaseService(null, null, null);
            signalPhaseService.LinkPivotAddSeconds(signalPhase, secondsToAdd);

            // Assert
            foreach (var cyclePcd in signalPhase.Cycles)
            {
                foreach (var detectorEvent in cyclePcd.DetectorEvents)
                {
                    var expectedTime = detectorEvent.TimeStamp.AddSeconds(secondsToAdd);
                    Assert.AreEqual(expectedTime, detectorEvent.TimeStamp);
                }
            }
        }
    }
}