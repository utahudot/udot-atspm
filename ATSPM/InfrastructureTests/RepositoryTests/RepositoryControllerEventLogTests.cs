﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - %Namespace%/RepositoryControllerEventLogTests.cs
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

using Utah.Udot.Atspm.Data.Models
using Utah.Udot.Atspm.Repositories;
using Utah.Udot.NetStandardToolkit.Services;
using Atspm.Infrastructure.Data;
using Atspm.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Utah.Udot.NetStandardToolkit.Extensions;
using Utah.Udot.Atspm.Specifications;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.ValueObjects;

namespace InfrastructureTests
{
    public class RepositoryControllerEventLogTests
    {
        private readonly DbConnection _connection = new SqliteConnection("Datasource=:memory:");

        //const string connectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

        private readonly ITestOutputHelper _output;
        private ILogger<ControllerEventLogEFRepository> _nullLogger;
        private MOEContext _db;
        private IControllerEventLogRepository _repo;

        public RepositoryControllerEventLogTests(ITestOutputHelper output)
        {
            _output = output;
            _nullLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ControllerEventLogEFRepository>();

            //_connection.Open();
            _db = new MOEContext(new DbContextOptionsBuilder<MOEContext>().EnableSensitiveDataLogging().UseSqlite(_connection).Options);

            _repo = new ControllerEventLogEFRepository(_db, _nullLogger);

            _db.Database.EnsureDeleted();

            _output.WriteLine($"Created database: {_db.Database.EnsureCreated()}");
        }

        [Fact(Skip = "Not ready yet")]
        public void CheckForRecords()
        {
        }

        [Fact]
        public async void GetAllAggregationCodes()
        {
            await SeedData();

            var locationId = "1001";
            var startTime = DateTime.Today;
            var endTime = DateTime.Today.AddDays(1);


            var expected = codes.OrderBy(o => o).ToList();
            var actual = _repo.GetAllAggregationCodes(locationId, startTime, endTime).Select(s => s.EventCode).Distinct().OrderBy(o => o).ToList();

            _output.WriteLine($"expected: {expected.Count}");
            _output.WriteLine($"actual: {actual.Count}");

            Assert.Equal(expected, actual);
        }

        [Fact(Skip = "Not ready yet")]
        public void GetApproachEventsCountBetweenDates()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetDetectorActivationCount()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetEventCountByEventCodesParamDateTimeRange()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetEventsBetweenDates()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetEventsByEventCodesParamOverloadOne()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetEventsByEventCodesParamOverloadTwo()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetEventsByEventCodesParamOverloadTHree()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetEventsByEventCodesParamDateTimeRange()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetEventsByEventCodesParamWithLatencyCorrection()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetEventsByEventCodesParamWithOffsetAndLatencyCorrection()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetFirstEventBeforeDate()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetFirstEventBeforeDateByEventCodeAndParameter()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetMostRecentRecordTimestamp()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetRecordCount()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetRecordCountByParameterAndEvent()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetRecordsByParameterAndEvent()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetLocationEventsBetweenDates()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetLocationEventsByEventCode()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetLocationEventsByEventCodes()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetLocationEventsCountBetweenDates()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetSplitEvents()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetTmcVolume()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetTopEventsAfterDateByEventCodesParam()
        {
        }

        [Fact(Skip = "Not ready yet")]
        public void GetTopNumberOfLocationEventsBetweenDates()
        {
        }

        private static List<int> codes = new List<int> { 150, 114, 113, 112, 105, 102, 1 };
        private static List<int> approachCodes = new List<int> { 1, 8, 10 };

        private async Task SeedData()
        {
            Random random = new Random();

            for (int x = 1; x <= random.Next(5, 25); x++)
            {
                for (int z = 0; z <= 4; z++)
                {
                    var controlLogArchive = new ControllerLogArchive() { locationId = (x + 1000).ToString(), ArchiveDate = DateTime.Today.AddDays(z) };

                    controlLogArchive.LogData = new List<ControllerEventLog>();

                    for (int y = 0; y <= random.Next(50, 100); y++)
                    {
                        controlLogArchive.LogData.Add(new ControllerEventLog() { EventCode = codes[random.Next(0, codes.Count)], EventParam = random.Next(1, 255), Timestamp = controlLogArchive.ArchiveDate });
                    }

                    await _db.Set<ControllerLogArchive>().AddAsync(controlLogArchive);
                }
            }

            await _db.SaveChangesAsync();

            //await foreach (var i in _db.Set<ControllerLogArchive>().OrderBy(o => o.locationId).AsAsyncEnumerable())
            //{
            //    _output.WriteLine($"item: {i.locationId} : {i.ArchiveDate} : {i.LogData.Count()}");
            //}
        }
    }
}
