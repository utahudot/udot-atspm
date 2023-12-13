using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Infrastructure.Repositories;
using AutoFixture;
using InfrastructureTests.Fixtures;
using InfrastructureTests.RepositoryTests;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace InfrastructureTests.RepositoryTests
{
    //[TestCaseOrderer("InfrastructureTests.Orderers.TraitValueTestCaseOrderer", "InfrastructureTests")]
    public class IControllerEventLogRepositoryTests : RepositoryTestBase<ControllerLogArchive, IControllerEventLogRepository, EventLogContext>
    {
        private const int ItemCount = 5;
        private const int Logount = 5;

        private List<ControllerLogArchive> _list = new List<ControllerLogArchive>();

        public IControllerEventLogRepositoryTests(EFContextFixture<EventLogContext> dbFixture, ITestOutputHelper output) : base(dbFixture, output) { }

        protected override async void SeedTestData()
        {
            if (_repo.GetList().Count() < ItemCount)
            {
                for (int x = 1; x <= ItemCount; x++)
                {
                    var f = ModelFixture.Build<ControllerLogArchive>()
                        .With(w => w.locationIdentifier, $"{x + 1000}")
                        .With(w => w.ArchiveDate, DateTime.Today.AddDays((x - (x * 2)) - 1))
                        .Without(w => w.LogData).Create();

                    for (int y = 1; y <= Logount; y++)
                    {
                        f.LogData.Add(ModelFixture.Build<ControllerEventLog>()
                        .With(w => w.locationIdentifier, f.locationIdentifier)
                        .With(w => w.TimeStamp, f.ArchiveDate.AddMinutes(y * 10))
                        .Create());
                    }

                    await _repo.AddAsync(f);
                }
            }
            _list = _repo.GetList().ToList();

            foreach (var s in _list)
            {
                _output.WriteLine($"Seed Data: {s.locationIdentifier} - {s.ArchiveDate} - {s.LogData.Count}");

                foreach (var l in s.LogData)
                {
                    _output.WriteLine($"LogData: {l.locationIdentifier} - {l.TimeStamp} - {l.EventCode} - {l.EventParam}");

                }

            }
        }

        #region IControllerEventLogRepository

        [Fact]
        public void IControllerEventLogRepositoryGetLocationEventsBetweenDates()
        {
            var locationId = "1001";
            var start = DateTime.Today.AddDays(-2);
            var end = start.AddDays(1).AddSeconds(-1);

            _output.WriteLine($"date range: {start} - {end}");

            var result = _repo.GetLocationEventsBetweenDates(locationId, start, end);

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.locationIdentifier} - {r.TimeStamp} - {r.EventCode} - {r.EventParam}");
            }

            //locationId should equal locationId
            Assert.True(result.All(a => a.locationIdentifier == locationId));

            //timestamp should be between start/end dates
            Assert.True(result.All(a => a.TimeStamp >= start && a.TimeStamp <= end));
        }

        #endregion

        #region IControllerEventLogRepositoryExtensions

        //[Fact]
        //public void IControllerEventLogRepositoryGetEventsByEventCodesParam()
        //{
        //    var locationId = "1001";
        //    var timestamp = DateTime.Now;
        //    var start = timestamp.AddMinutes(-1);
        //    var end = timestamp.AddMinutes(1);

        //    List<ControllerEventLog> eventLogs = new List<ControllerEventLog>();

        //    for(int x = 1; x <= 100; x++)
        //    {
        //        for(int y = 1; y <= 100; y++)
        //        {
        //            eventLogs.Add(new ControllerEventLog() { locationId = locationId, Timestamp = timestamp, EventCode = x, EventParam = y });
        //        }
        //    }


        //    var sut = new Mock<IControllerEventLogRepository>();
        //    sut.Setup(r => r.GetLocationEventsBetweenDates(locationId, start, end)).Returns(() => eventLogs);

        //    _output.WriteLine($"date range: {start} - {end}");

        //    var result = sut.Object.GetEventsByEventCodesParam(locationId, start, end, Enumerable.Range(1, 5), 1);

        //    foreach (var r in result)
        //    {
        //        _output.WriteLine($"result: {r.locationId} - {r.Timestamp} - {r.EventCode} - {r.EventParam}");
        //    }

        //    //locationId should equal locationId
        //    Assert.True(result.All(a => a.locationId == locationId));

        //    //timestamp should be between start/end dates
        //    Assert.True(result.All(a => a.Timestamp >= start && a.Timestamp <= end));
        //}

        #endregion
    }
}
