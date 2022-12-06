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
    public class IActionLogRepositoryTests : IClassFixture<EFContextFixture<ConfigContext>>
    {
        private EFContextFixture<ConfigContext> _db;
        private readonly ITestOutputHelper _output;
        private IActionLogRepository _repo;

        private List<ActionLog> _list = new List<ActionLog>();

        public IActionLogRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output)
        {
            _db = dbFixture;
            _output = output;
            
            _repo = new ActionLogEFRepository(_db.Context, new Microsoft.Extensions.Logging.Abstractions.NullLogger<ActionLogEFRepository>());

            SeedTestData();
        }

        private async void SeedTestData()
        {
            if (_repo.GetList().Count() < 1)
            {
                var fixture = new Fixture();

                fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());

                fixture.Customize<ActionLog>(c => c
                    .Without(w => w.Id)
                    .Without(w => w.Agency)
                    .Without(w => w.Actions)
                    .Without(w => w.MetricTypes)
                );

                for (int x = 1; x <= Enum.GetValues(typeof(AgencyTypes)).Length - 1; x++)
                {
                    var f = fixture.Create<ActionLog>();
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
        public async void IActionLogRepositoryGetAllByDate()
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
