using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Infrastructure.Repositories;
using AutoFixture;
using InfrastructureTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace InfrastructureTests.RepositoryTests
{
    //[TestCaseOrderer("InfrastructureTests.Orderers.TraitValueTestCaseOrderer", "InfrastructureTests")]
    public class IActionLogRepositoryTests : RepositoryTestBase<ActionLog, IActionLogRepository, ConfigContext>
    {
        private List<ActionLog> _list = new List<ActionLog>();

        public IActionLogRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output) : base(dbFixture, output) { }

        protected override async void SeedTestData()
        {
            if (_repo.GetList().Count() < 1)
            {
                for (int x = 1; x <= Enum.GetValues(typeof(AgencyTypes)).Length - 1; x++)
                {
                    var f = ModelFixture.Create<ActionLog>();
                    f.Date = DateTime.Today.AddDays((x - (x * 2)) - 1);
                    f.AgencyId = (AgencyTypes)x;

                    await _repo.AddAsync(f);
                }
            }

            _list = _repo.GetList().ToList();

            foreach (var s in _list)
            {
                _output.WriteLine($"Seed Data: {s.Id} - {s.Name} - {s.Date} - {s.AgencyId} - {s.SignalId} - {s.Comment}");
            }
        }

        #region IActionLogRepository

        #endregion

        #region IActionLogRepositoryExtensions

        [Fact]
        public void IActionLogRepositoryGetAllByDate()
        {
            var start = DateTime.Today.AddDays(-4);
            var end = DateTime.Today;

            var result = _repo.GetAllByDate(start, end);

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Id} - {r.Name} - {r.Date} - {r.AgencyId} - {r.SignalId} - {r.Comment}");
            }

            //assert date range
            Assert.True(result.All(a => a.Date >= start && a.Date <= end));

            //assert return count
            Assert.Equal(3, result.Count);

            //compare to initial collection
            Assert.Equal(_list.Where(a => a.Date >= start && a.Date <= end), result);
        }

        #endregion
    }
}
