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
    public class IDetectorRepositoryTests : RepositoryTestBase<Detector, IDetectorRepository, ConfigContext, int>
    {
        private const int ItemCount = 1;

        private List<Detector> _list = new List<Detector>();

        public IDetectorRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output) : base(dbFixture, output) { }

        protected override async void SeedTestData()
        {
            if (_repo.GetList().Count() < ItemCount)
            {
                var s = ModelFixture.Create<Signal>();

                var a = ModelFixture.Create<Approach>();
                a.Signal = s;

                for (int x = 1; x <= ItemCount; x++)
                {
                    var d = ModelFixture.Create<Detector>();
                    d.Approach = a;

                    await _repo.AddAsync(d);
                }
            }

            _list = _repo.GetList().ToList();

            foreach (var s in _list)
            {
                _output.WriteLine($"Seed Data: {s.Id} - {s.Approach} - {s.DetectionHardware} - {s.LaneType} - {s.MovementType}");
            }
        }

        #region IDetectorRepository

        #endregion

        #region IDetectorRepositoryExtensions

        [Fact(Skip = "Not Complete")]
        public void IDetectorRepositoryGetMostRecentDetectorByDetectorID()
        {
            Assert.False(true);
        }

        #endregion
    }
}