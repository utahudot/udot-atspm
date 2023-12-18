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
    public class IDetectorCommentRepositoryTests : RepositoryTestBase<DetectorComment, IDetectorCommentRepository, ConfigContext, int>
    {
        private const int ItemCount = 4;
        private const int DetectorId = 1;

        private List<DetectorComment> _list = new List<DetectorComment>();

        public IDetectorCommentRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output) : base(dbFixture, output) { }

        protected override async void SeedTestData()
        {
            if (_repo.GetList().Count() < ItemCount)
            {
                var s = ModelFixture.Create<Location>();

                var a = ModelFixture.Create<Approach>();
                a.Location = s;

                var d = ModelFixture.Create<Detector>();
                d.Id = DetectorId;
                d.Approach = a;

                for (int x = 1; x <= ItemCount; x++)
                {
                    for (int y = 1; y <= ItemCount; y++)
                    {
                        var f = ModelFixture.Create<DetectorComment>();
                        //f.Id = x;
                        f.TimeStamp = DateTime.Today.AddDays((y - (y * 2)) - 1);
                        f.DetectorId = d.Id;
                        f.Detector = d;

                        await _repo.AddAsync(f);
                    }
                }
            }

            _list = _repo.GetList().ToList();

            foreach (var s in _list)
            {
                _output.WriteLine($"Seed Data: {s.Id} - {s.Comment} - {s.TimeStamp}");
            }
        }

        #region IDetectorCommentRepository

        #endregion

        #region IDetectorCommentRepositoryExtensions

        [Fact]
        public void IDetectorCommentRepositoryGetMostRecentDetectorCommentByDetectorID()
        {
            var result = _repo.GetMostRecentDetectorCommentByDetectorID(DetectorId);

            _output.WriteLine($"result: {result.Id} - {result.Comment} - {result.TimeStamp}");

            var expected = _list.Where(r => r.DetectorId == DetectorId).Select(s => s.TimeStamp).Max();
            var actual = result.TimeStamp;

            //timestamp should be most recent
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}