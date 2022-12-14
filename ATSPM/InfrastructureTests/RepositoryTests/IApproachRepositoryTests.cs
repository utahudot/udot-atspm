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
    public class IApproachRepositoryTests : RepositoryTestBase<Approach, IApproachRepository, ConfigContext>
    {
        private List<Approach> _list = new List<Approach>();

        public IApproachRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output) : base(dbFixture, output) { }

        protected override async void SeedTestData()
        {
            if (_repo.GetList().Count() < 1)
            {
                for (int x = 1; x <= Enum.GetValues(typeof(DirectionTypes)).Length - 1; x++)
                {
                    var s = ModelFixture.Create<Signal>();
                    s.Id = x + 1000;
                    
                    var f = ModelFixture.Create<Approach>();
                    f.SignalId = s.Id;
                    f.Signal = s;

                    await _repo.AddAsync(f);
                }
            }

            _list = _repo.GetList().ToList();

            foreach (var s in _list)
            {
                _output.WriteLine($"Seed Data: {s.Id} - {s.SignalId} - {s.Description} - {s.DirectionTypeId}");
            }
        }

        #region IApproachRepository

        #endregion

        #region IApproachRepositoryExtensions

        [Fact]
        public void IApproachRepositoryGetApproachesByIds()
        {
            var result = _repo.GetApproachesByIds(_list.Select(i => i.Id));

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Id} - {r.SignalId} - {r.Description} - {r.DirectionTypeId}");
            }

            //compare to initial collection
            Assert.Equal(_list, result);
        }

        #endregion
    }
}
