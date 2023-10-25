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
    public class ISignalRepositoryTests : RepositoryTestBase<Signal, ISignalRepository, ConfigContext, int>
    {
        private const int ItemCount = 4;
        private const int ControllerTypeId = 1;

        private List<Signal> _signalList = new List<Signal>();

        public ISignalRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output) : base(dbFixture, output) { }

        protected override async void SeedTestData()
        {
            if (_repo.GetList().Count() < 1)
            {
                for (int x = 1; x <= ItemCount; x++)
                {
                    string signalId = x.ToString();

                    for (int i = 0; i <= Enum.GetValues(typeof(SignalVersionActions)).Length - 2; i++)
                    {
                        var s = ModelFixture.Create<Signal>();
                        s.SignalIdentifier = signalId;
                        s.VersionActionId = (SignalVersionActions)i;
                        s.PrimaryName = s.VersionActionId.ToString();
                        s.VersionAction = _db.Context.Set<VersionAction>().Find(s.VersionActionId);
                        s.ControllerTypeId = (i % 2 == 0) ? 1 : 2;
                        s.Start = DateTime.Today.AddDays((i - (i * 2)) - 1);
                        await _repo.AddAsync(s);
                    }
                }
            }

            _signalList = _repo.GetList().ToList();

            foreach (var s in _signalList)
            {
                _output.WriteLine($"Seed Data: {s.Id} - {s.SignalIdentifier} - {s.PrimaryName} - {s.VersionActionId} - {s.Start} - {s.ControllerTypeId}");
            }
        }

        #region ISignalRepository

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public void ISignalRepositoryGetAllVersionsOfSignal(string signalId)
        {
            var result = _repo.GetAllVersionsOfSignal(signalId);

            foreach (var s in result)
            {
                _output.WriteLine($"result: {s.Id} - {s.SignalIdentifier} - {s.PrimaryName} - {s.VersionActionId} - {s.Start}");
            }

            //should not return deleted signals
            Assert.True(!result.Select(s => s.VersionActionId).Contains(SignalVersionActions.Delete));

            //all values should be signalId
            Assert.True(result.All(a => a.SignalIdentifier == signalId));

            //values should be sorted by start date
            Assert.Equal(result.Select(s => s.Start).OrderByDescending(o => o), result.Select(s => s.Start));
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public void ISignalRepositoryGetLatestVersionOfSignal(string signalId)
        {
            var result = _repo.GetLatestVersionOfSignal(signalId);

            _output.WriteLine($"result: {result.Id} - {result.SignalIdentifier} - {result.PrimaryName} - {result.VersionActionId} - {result.Start}");

            //should not return deleted signals
            Assert.True(result.VersionActionId != SignalVersionActions.Delete);

            //all values should be signalId
            Assert.True(result.SignalIdentifier == signalId);

            //value should be newest date
            Assert.Equal(_signalList.Where(w => w.VersionActionId != SignalVersionActions.Delete && w.SignalIdentifier == signalId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), result.Start);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public void ISignalRepositoryGetLatestVersionOfSignalWithDate(string signalId)
        {
            var start = DateTime.Today.AddDays(-2);

            var result = _repo.GetLatestVersionOfSignal(signalId, start);

            _output.WriteLine($"result: {result.Id} - {result.SignalIdentifier} - {result.PrimaryName} - {result.VersionActionId} - {result.Start}");

            //should not return deleted signals
            Assert.True(result.VersionActionId != SignalVersionActions.Delete);

            //all values should be signalId
            Assert.True(result.SignalIdentifier == signalId);

            //should all be <= start date
            Assert.True(result.Start <= start);

            //value should be newest date
            Assert.Equal(_signalList.Where(w => w.VersionActionId != SignalVersionActions.Delete && w.SignalIdentifier == signalId && w.Start <= start).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), result.Start);
        }

        [Fact]
        public void ISignalRepositoryGetLatestVersionOfAllSignals()
        {
            var result = _repo.GetLatestVersionOfAllSignals();

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Id} - {r.SignalIdentifier} - {r.PrimaryName} - {r.VersionActionId} - {r.Start}");
            }

            //should not return deleted signals
            Assert.True(result.All(a => a.VersionActionId != SignalVersionActions.Delete));

            //result list should equal signalCount
            Assert.True(result.Count == ItemCount);

            //value should be newest date
            Assert.Collection(result,
                i => Assert.Equal(_signalList.Where(w => w.SignalIdentifier == i.SignalIdentifier && w.VersionActionId != SignalVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalIdentifier == i.SignalIdentifier && w.VersionActionId != SignalVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalIdentifier == i.SignalIdentifier && w.VersionActionId != SignalVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalIdentifier == i.SignalIdentifier && w.VersionActionId != SignalVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start));
        }

        [Fact]
        public void ISignalRepositoryGetLatestVersionOfAllSignalsByControllerTypeId()
        {
            var result = _repo.GetLatestVersionOfAllSignals(ControllerTypeId);

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Id} - {r.SignalIdentifier} - {r.PrimaryName} - {r.VersionActionId} - {r.Start} - {r.ControllerTypeId}");
            }

            //controller type should all equal controllerTypeId
            Assert.True(result.All(a => a.ControllerTypeId == ControllerTypeId));

            //should not return deleted signals
            Assert.True(result.All(a => a.VersionActionId != SignalVersionActions.Delete));

            //result list should equal signalCount
            Assert.True(result.Count == ItemCount);

            //value should be newest date
            Assert.Collection(result,
                i => Assert.Equal(_signalList.Where(w => w.SignalIdentifier == i.SignalIdentifier && w.VersionActionId != SignalVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalIdentifier == i.SignalIdentifier && w.VersionActionId != SignalVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalIdentifier == i.SignalIdentifier && w.VersionActionId != SignalVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalIdentifier == i.SignalIdentifier && w.VersionActionId != SignalVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start));
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public void ISignalRepositoryGetSignalsBetweenDates(string signalId)
        {
            var start = DateTime.Today.AddDays(-2);
            var end = DateTime.Today;

            var result = _repo.GetSignalsBetweenDates(signalId, start, end);

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Id} - {r.SignalIdentifier} - {r.PrimaryName} - {r.VersionActionId} - {r.Start}");
            }

            //should not return deleted signals
            Assert.True(result.All(a => a.VersionActionId != SignalVersionActions.Delete));

            //should all be signalId
            Assert.True(result.All(a => a.SignalIdentifier == signalId));

            //should all be between start and end dates
            Assert.True(result.All(a => a.Start > start && a.Start < end));

            //compare to initial collection
            Assert.Equal(_signalList.Where(w => w.SignalIdentifier == signalId && w.VersionActionId != SignalVersionActions.Delete && w.Start > start && w.Start < end), result);
        }

        #endregion

        #region ISignalRepositoryExtensions

        //[Theory, AutoDataOmitRecursion]
        //[Trait("Key", "Value")]
        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public async void ISignalRepositoryCopySignalToNewVersion(string signalId)
        {
            var signal = _repo.GetLatestVersionOfSignal(signalId);

            var actual = await _repo.CopySignalToNewVersion(signal.Id);

            _output.WriteLine($"Compare: {signal.Id} - {actual.Id}");
            _output.WriteLine($"Compare: {signal.SignalIdentifier} - {actual.SignalIdentifier}");
            _output.WriteLine($"Compare: {signal.Latitude} - {actual.Latitude}");
            _output.WriteLine($"Compare: {signal.Longitude} - {actual.Longitude}");
            _output.WriteLine($"Compare: {signal.PrimaryName} - {actual.PrimaryName}");
            _output.WriteLine($"Compare: {signal.SecondaryName} - {actual.SecondaryName}");
            _output.WriteLine($"Compare: {signal.Ipaddress} - {actual.Ipaddress}");
            _output.WriteLine($"Compare: {signal.RegionId} - {actual.RegionId}");
            _output.WriteLine($"Compare: {signal.ControllerTypeId} - {actual.ControllerTypeId}");
            _output.WriteLine($"Compare: {signal.ChartEnabled} - {actual.ChartEnabled}");
            _output.WriteLine($"Compare: {signal.VersionActionId} - {actual.VersionActionId}");
            _output.WriteLine($"Compare: {signal.Note} - {actual.Note}");
            _output.WriteLine($"Compare: {signal.Start} - {actual.Start}");
            _output.WriteLine($"Compare: {signal.JurisdictionId} - {actual.JurisdictionId}");
            _output.WriteLine($"Compare: {signal.Pedsare1to1} - {actual.Pedsare1to1}");

            Assert.NotEqual(expected: signal.Id, actual: actual.Id);
            Assert.Equal(expected: signal.SignalIdentifier, actual: actual.SignalIdentifier);
            Assert.Equal(expected: signal.Latitude, actual: actual.Latitude);
            Assert.Equal(expected: signal.Longitude, actual: actual.Longitude);
            Assert.Equal(expected: signal.PrimaryName, actual: actual.PrimaryName);
            Assert.Equal(expected: signal.SecondaryName, actual: actual.SecondaryName);
            Assert.Equal(expected: signal.Ipaddress.ToString(), actual: actual.Ipaddress.ToString());
            Assert.Equal(expected: signal.RegionId, actual: actual.RegionId);
            Assert.Equal(expected: signal.ControllerTypeId, actual: actual.ControllerTypeId);
            Assert.Equal(expected: signal.ChartEnabled, actual: actual.ChartEnabled);
            Assert.Equal(expected: SignalVersionActions.NewVersion, actual: actual.VersionActionId);
            Assert.Contains("Copy of", actual.Note);
            Assert.Equal(expected: DateTime.Today, actual: actual.Start);
            Assert.Equal(expected: signal.JurisdictionId, actual: actual.JurisdictionId);
            Assert.Equal(expected: signal.Pedsare1to1, actual: actual.Pedsare1to1);
            Assert.True(actual.Start > signal.Start);
        }

        [Fact]
        public async void ISignalRepositorySetSignalToDeleted()
        {
            var expected = _repo.GetList().FirstOrDefault(s => s.VersionActionId != SignalVersionActions.Delete);

            _output.WriteLine($"Original: {expected.VersionActionId}");

            await _repo.SetSignalToDeleted(expected.Id);

            var actual = await _repo.LookupAsync(expected.Id);

            _output.WriteLine($"Modified: {actual.VersionActionId}");

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
