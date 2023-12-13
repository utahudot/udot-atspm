using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
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
    public class ILocationRepositoryTests : RepositoryTestBase<Location, ILocationRepository, ConfigContext, int>
    {
        private const int ItemCount = 4;
        private const int ControllerTypeId = 1;

        private List<Location> _LocationList = new List<Location>();

        public ILocationRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output) : base(dbFixture, output) { }

        protected override async void SeedTestData()
        {
            if (_repo.GetList().Count() < 1)
            {
                for (int x = 1; x <= ItemCount; x++)
                {
                    string locationId = x.ToString();

                    for (int i = 0; i <= Enum.GetValues(typeof(LocationVersionActions)).Length - 2; i++)
                    {
                        var s = ModelFixture.Create<Location>();
                        s.LocationIdentifier = locationId;
                        s.VersionAction = (LocationVersionActions)i;
                        s.PrimaryName = s.VersionAction.ToString();
                        s.ControllerTypeId = (i % 2 == 0) ? 1 : 2;
                        s.Start = DateTime.Today.AddDays((i - (i * 2)) - 1);
                        await _repo.AddAsync(s);
                    }
                }
            }

            _LocationList = _repo.GetList().ToList();

            foreach (var s in _LocationList)
            {
                _output.WriteLine($"Seed Data: {s.Id} - {s.LocationIdentifier} - {s.PrimaryName} - {s.VersionAction} - {s.Start} - {s.ControllerTypeId}");
            }
        }

        #region ILocationRepository

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public void ILocationRepositoryGetAllVersionsOfLocation(string locationId)
        {
            var result = _repo.GetAllVersionsOfLocation(locationId);

            foreach (var s in result)
            {
                _output.WriteLine($"result: {s.Id} - {s.LocationIdentifier} - {s.PrimaryName} - {s.VersionAction} - {s.Start}");
            }

            //should not return deleted Locations
            Assert.True(!result.Select(s => s.VersionAction).Contains(LocationVersionActions.Delete));

            //all values should be locationId
            Assert.True(result.All(a => a.LocationIdentifier == locationId));

            //values should be sorted by start date
            Assert.Equal(result.Select(s => s.Start).OrderByDescending(o => o), result.Select(s => s.Start));
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public void ILocationRepositoryGetLatestVersionOfLocation(string locationId)
        {
            var result = _repo.GetLatestVersionOfLocation(locationId);

            _output.WriteLine($"result: {result.Id} - {result.LocationIdentifier} - {result.PrimaryName} - {result.VersionAction} - {result.Start}");

            //should not return deleted Locations
            Assert.True(result.VersionAction != LocationVersionActions.Delete);

            //all values should be locationId
            Assert.True(result.LocationIdentifier == locationId);

            //value should be newest date
            Assert.Equal(_LocationList.Where(w => w.VersionAction != LocationVersionActions.Delete && w.LocationIdentifier == locationId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), result.Start);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public void ILocationRepositoryGetLatestVersionOfLocationWithDate(string locationId)
        {
            var start = DateTime.Today.AddDays(-2);

            var result = _repo.GetLatestVersionOfLocation(locationId, start);

            _output.WriteLine($"result: {result.Id} - {result.LocationIdentifier} - {result.PrimaryName} - {result.VersionAction} - {result.Start}");

            //should not return deleted Locations
            Assert.True(result.VersionAction != LocationVersionActions.Delete);

            //all values should be locationId
            Assert.True(result.LocationIdentifier == locationId);

            //should all be <= start date
            Assert.True(result.Start <= start);

            //value should be newest date
            Assert.Equal(_LocationList.Where(w => w.VersionAction != LocationVersionActions.Delete && w.LocationIdentifier == locationId && w.Start <= start).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), result.Start);
        }

        [Fact]
        public void ILocationRepositoryGetLatestVersionOfAllLocations()
        {
            var result = _repo.GetLatestVersionOfAllLocations();

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Id} - {r.LocationIdentifier} - {r.PrimaryName} - {r.VersionAction} - {r.Start}");
            }

            //should not return deleted Locations
            Assert.True(result.All(a => a.VersionAction != LocationVersionActions.Delete));

            //result list should equal LocationCount
            Assert.True(result.Count == ItemCount);

            //value should be newest date
            Assert.Collection(result,
                i => Assert.Equal(_LocationList.Where(w => w.LocationIdentifier == i.LocationIdentifier && w.VersionAction != LocationVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_LocationList.Where(w => w.LocationIdentifier == i.LocationIdentifier && w.VersionAction != LocationVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_LocationList.Where(w => w.LocationIdentifier == i.LocationIdentifier && w.VersionAction != LocationVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_LocationList.Where(w => w.LocationIdentifier == i.LocationIdentifier && w.VersionAction != LocationVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start));
        }

        [Fact]
        public void ILocationRepositoryGetLatestVersionOfAllLocationsByControllerTypeId()
        {
            var result = _repo.GetLatestVersionOfAllLocations(ControllerTypeId);

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Id} - {r.LocationIdentifier} - {r.PrimaryName} - {r.VersionAction} - {r.Start} - {r.ControllerTypeId}");
            }

            //controller type should all equal controllerTypeId
            Assert.True(result.All(a => a.ControllerTypeId == ControllerTypeId));

            //should not return deleted Locations
            Assert.True(result.All(a => a.VersionAction != LocationVersionActions.Delete));

            //result list should equal LocationCount
            Assert.True(result.Count == ItemCount);

            //value should be newest date
            Assert.Collection(result,
                i => Assert.Equal(_LocationList.Where(w => w.LocationIdentifier == i.LocationIdentifier && w.VersionAction != LocationVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_LocationList.Where(w => w.LocationIdentifier == i.LocationIdentifier && w.VersionAction != LocationVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_LocationList.Where(w => w.LocationIdentifier == i.LocationIdentifier && w.VersionAction != LocationVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_LocationList.Where(w => w.LocationIdentifier == i.LocationIdentifier && w.VersionAction != LocationVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start));
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public void ILocationRepositoryGetLocationsBetweenDates(string locationId)
        {
            var start = DateTime.Today.AddDays(-2);
            var end = DateTime.Today;

            var result = _repo.GetLocationsBetweenDates(locationId, start, end);

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Id} - {r.LocationIdentifier} - {r.PrimaryName} - {r.VersionAction} - {r.Start}");
            }

            //should not return deleted Locations
            Assert.True(result.All(a => a.VersionAction != LocationVersionActions.Delete));

            //should all be locationId
            Assert.True(result.All(a => a.LocationIdentifier == locationId));

            //should all be between start and end dates
            Assert.True(result.All(a => a.Start > start && a.Start < end));

            //compare to initial collection
            Assert.Equal(_LocationList.Where(w => w.LocationIdentifier == locationId && w.VersionAction != LocationVersionActions.Delete && w.Start > start && w.Start < end), result);
        }

        #endregion

        #region ILocationRepositoryExtensions

        //[Theory, AutoDataOmitRecursion]
        //[Trait("Key", "Value")]
        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public async void ILocationRepositoryCopyLocationToNewVersion(string locationId)
        {
            var Location = _repo.GetLatestVersionOfLocation(locationId);

            var actual = await _repo.CopyLocationToNewVersion(Location.Id);

            _output.WriteLine($"Compare: {Location.Id} - {actual.Id}");
            _output.WriteLine($"Compare: {Location.LocationIdentifier} - {actual.LocationIdentifier}");
            _output.WriteLine($"Compare: {Location.Latitude} - {actual.Latitude}");
            _output.WriteLine($"Compare: {Location.Longitude} - {actual.Longitude}");
            _output.WriteLine($"Compare: {Location.PrimaryName} - {actual.PrimaryName}");
            _output.WriteLine($"Compare: {Location.SecondaryName} - {actual.SecondaryName}");
            _output.WriteLine($"Compare: {Location.Ipaddress} - {actual.Ipaddress}");
            _output.WriteLine($"Compare: {Location.RegionId} - {actual.RegionId}");
            _output.WriteLine($"Compare: {Location.ControllerTypeId} - {actual.ControllerTypeId}");
            _output.WriteLine($"Compare: {Location.ChartEnabled} - {actual.ChartEnabled}");
            _output.WriteLine($"Compare: {Location.VersionAction} - {actual.VersionAction}");
            _output.WriteLine($"Compare: {Location.Note} - {actual.Note}");
            _output.WriteLine($"Compare: {Location.Start} - {actual.Start}");
            _output.WriteLine($"Compare: {Location.JurisdictionId} - {actual.JurisdictionId}");
            _output.WriteLine($"Compare: {Location.PedsAre1to1} - {actual.PedsAre1to1}");

            Assert.NotEqual(expected: Location.Id, actual: actual.Id);
            Assert.Equal(expected: Location.LocationIdentifier, actual: actual.LocationIdentifier);
            Assert.Equal(expected: Location.Latitude, actual: actual.Latitude);
            Assert.Equal(expected: Location.Longitude, actual: actual.Longitude);
            Assert.Equal(expected: Location.PrimaryName, actual: actual.PrimaryName);
            Assert.Equal(expected: Location.SecondaryName, actual: actual.SecondaryName);
            Assert.Equal(expected: Location.Ipaddress.ToString(), actual: actual.Ipaddress.ToString());
            Assert.Equal(expected: Location.RegionId, actual: actual.RegionId);
            Assert.Equal(expected: Location.ControllerTypeId, actual: actual.ControllerTypeId);
            Assert.Equal(expected: Location.ChartEnabled, actual: actual.ChartEnabled);
            Assert.Equal(expected: LocationVersionActions.NewVersion, actual: actual.VersionAction);
            Assert.Contains("Copy of", actual.Note);
            Assert.Equal(expected: DateTime.Today, actual: actual.Start);
            Assert.Equal(expected: Location.JurisdictionId, actual: actual.JurisdictionId);
            Assert.Equal(expected: Location.PedsAre1to1, actual: actual.PedsAre1to1);
            Assert.True(actual.Start > Location.Start);
        }

        [Fact]
        public async void ILocationRepositorySetLocationToDeleted()
        {
            var expected = _repo.GetList().FirstOrDefault(s => s.VersionAction != LocationVersionActions.Delete);

            _output.WriteLine($"Original: {expected.VersionAction}");

            await _repo.SetLocationToDeleted(expected.Id);

            var actual = await _repo.LookupAsync(expected.Id);

            _output.WriteLine($"Modified: {actual.VersionAction}");

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
