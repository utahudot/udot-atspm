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
    public class ISignalRepositoryTests : RepositoryTestBase<Signal, ISignalRepository, ConfigContext>
    {
        private const int SignalCount = 4;
        private const int ControllerTypeId = 1;

        private List<Signal> _signalList = new List<Signal>();

        public ISignalRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output) : base(dbFixture, output) { }

        protected override async void SeedTestData()
        {
            if (_repo.GetList().Count() < 1)
            {
                for (int x = 1; x <= SignalCount; x++)
                {
                    string signalId = x.ToString();

                    for (int i = 0; i <= Enum.GetValues(typeof(SignaVersionActions)).Length - 2; i++)
                    {
                        var s = ModelFixture.Create<Signal>();
                        s.SignalId = signalId;
                        s.VersionActionId = (SignaVersionActions)i;
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
                _output.WriteLine($"Seed Data: {s.Id} - {s.SignalId} - {s.PrimaryName} - {s.VersionActionId} - {s.Start} - {s.ControllerTypeId}");
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
                _output.WriteLine($"result: {s.Id} - {s.SignalId} - {s.PrimaryName} - {s.VersionActionId} - {s.Start}");
            }

            //should not return deleted signals
            Assert.True(!result.Select(s => s.VersionActionId).Contains(SignaVersionActions.Delete));

            //all values should be signalId
            Assert.True(result.All(a => a.SignalId == signalId));

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

            _output.WriteLine($"result: {result.Id} - {result.SignalId} - {result.PrimaryName} - {result.VersionActionId} - {result.Start}");

            //should not return deleted signals
            Assert.True(result.VersionActionId != SignaVersionActions.Delete);

            //all values should be signalId
            Assert.True(result.SignalId == signalId);

            //value should be newest date
            Assert.Equal(_signalList.Where(w => w.VersionActionId != SignaVersionActions.Delete && w.SignalId == signalId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), result.Start);
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

            _output.WriteLine($"result: {result.Id} - {result.SignalId} - {result.PrimaryName} - {result.VersionActionId} - {result.Start}");

            //should not return deleted signals
            Assert.True(result.VersionActionId != SignaVersionActions.Delete);

            //all values should be signalId
            Assert.True(result.SignalId == signalId);

            //should all be <= start date
            Assert.True(result.Start <= start);

            //value should be newest date
            Assert.Equal(_signalList.Where(w => w.VersionActionId != SignaVersionActions.Delete && w.SignalId == signalId && w.Start <= start).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), result.Start);
        }

        [Fact]
        public void ISignalRepositoryGetLatestVersionOfAllSignals()
        {
            var result = _repo.GetLatestVersionOfAllSignals();

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Id} - {r.SignalId} - {r.PrimaryName} - {r.VersionActionId} - {r.Start}");
            }

            //should not return deleted signals
            Assert.True(result.All(a => a.VersionActionId != SignaVersionActions.Delete));

            //result list should equal signalCount
            Assert.True(result.Count == SignalCount);

            //value should be newest date
            Assert.Collection(result,
                i => Assert.Equal(_signalList.Where(w => w.SignalId == i.SignalId && w.VersionActionId != SignaVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalId == i.SignalId && w.VersionActionId != SignaVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalId == i.SignalId && w.VersionActionId != SignaVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalId == i.SignalId && w.VersionActionId != SignaVersionActions.Delete).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start));
        }

        [Fact]
        public void ISignalRepositoryGetLatestVersionOfAllSignalsByControllerTypeId()
        {
            var result = _repo.GetLatestVersionOfAllSignals(ControllerTypeId);

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Id} - {r.SignalId} - {r.PrimaryName} - {r.VersionActionId} - {r.Start} - {r.ControllerTypeId}");
            }

            //controller type should all equal controllerTypeId
            Assert.True(result.All(a => a.ControllerTypeId == ControllerTypeId));

            //should not return deleted signals
            Assert.True(result.All(a => a.VersionActionId != SignaVersionActions.Delete));

            //result list should equal signalCount
            Assert.True(result.Count == SignalCount);

            //value should be newest date
            Assert.Collection(result,
                i => Assert.Equal(_signalList.Where(w => w.SignalId == i.SignalId && w.VersionActionId != SignaVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalId == i.SignalId && w.VersionActionId != SignaVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalId == i.SignalId && w.VersionActionId != SignaVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start),
                i => Assert.Equal(_signalList.Where(w => w.SignalId == i.SignalId && w.VersionActionId != SignaVersionActions.Delete && w.ControllerTypeId == ControllerTypeId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), i.Start));
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
                _output.WriteLine($"result: {r.Id} - {r.SignalId} - {r.PrimaryName} - {r.VersionActionId} - {r.Start}");
            }

            //should not return deleted signals
            Assert.True(result.All(a => a.VersionActionId != SignaVersionActions.Delete));

            //should all be signalId
            Assert.True(result.All(a => a.SignalId == signalId));

            //should all be between start and end dates
            Assert.True(result.All(a => a.Start > start && a.Start < end));

            //compare to initial collection
            Assert.Equal(_signalList.Where(w => w.SignalId == signalId && w.VersionActionId != SignaVersionActions.Delete && w.Start > start && w.Start < end), result);
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

            var actual = await _repo.CopySignalToNewVersion(signal);

            _output.WriteLine($"Compare: {signal.Id} - {actual.Id}");
            _output.WriteLine($"Compare: {signal.SignalId} - {actual.SignalId}");
            _output.WriteLine($"Compare: {signal.Latitude} - {actual.Latitude}");
            _output.WriteLine($"Compare: {signal.Longitude} - {actual.Longitude}");
            _output.WriteLine($"Compare: {signal.PrimaryName} - {actual.PrimaryName}");
            _output.WriteLine($"Compare: {signal.SecondaryName} - {actual.SecondaryName}");
            _output.WriteLine($"Compare: {signal.Ipaddress} - {actual.Ipaddress}");
            _output.WriteLine($"Compare: {signal.RegionId} - {actual.RegionId}");
            _output.WriteLine($"Compare: {signal.ControllerTypeId} - {actual.ControllerTypeId}");
            _output.WriteLine($"Compare: {signal.Enabled} - {actual.Enabled}");
            _output.WriteLine($"Compare: {signal.VersionActionId} - {actual.VersionActionId}");
            _output.WriteLine($"Compare: {signal.Note} - {actual.Note}");
            _output.WriteLine($"Compare: {signal.Start} - {actual.Start}");
            _output.WriteLine($"Compare: {signal.JurisdictionId} - {actual.JurisdictionId}");
            _output.WriteLine($"Compare: {signal.Pedsare1to1} - {actual.Pedsare1to1}");

            Assert.NotEqual(expected: signal.Id, actual: actual.Id);
            Assert.Equal(expected: signal.SignalId, actual: actual.SignalId);
            Assert.Equal(expected: signal.Latitude, actual: actual.Latitude);
            Assert.Equal(expected: signal.Longitude, actual: actual.Longitude);
            Assert.Equal(expected: signal.PrimaryName, actual: actual.PrimaryName);
            Assert.Equal(expected: signal.SecondaryName, actual: actual.SecondaryName);
            Assert.Equal(expected: signal.Ipaddress.ToString(), actual: actual.Ipaddress.ToString());
            Assert.Equal(expected: signal.RegionId, actual: actual.RegionId);
            Assert.Equal(expected: signal.ControllerTypeId, actual: actual.ControllerTypeId);
            Assert.Equal(expected: signal.Enabled, actual: actual.Enabled);
            Assert.Equal(expected: SignaVersionActions.NewVersion, actual: actual.VersionActionId);
            Assert.Contains("Copy of", actual.Note);
            Assert.Equal(expected: DateTime.Today, actual: actual.Start);
            Assert.Equal(expected: signal.JurisdictionId, actual: actual.JurisdictionId);
            Assert.Equal(expected: signal.Pedsare1to1, actual: actual.Pedsare1to1);
            Assert.True(actual.Start > signal.Start);
        }

        [Fact]
        public async void ISignalRepositorySetSignalToDeleted()
        {
            var expected = _repo.GetList().FirstOrDefault(s => s.VersionActionId != SignaVersionActions.Delete);

            _output.WriteLine($"Original: {expected.VersionActionId}");

            await _repo.SetSignalToDeleted(expected.Id);

            var actual = await _repo.LookupAsync(expected.Id);

            _output.WriteLine($"Modified: {actual.VersionActionId}");

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
