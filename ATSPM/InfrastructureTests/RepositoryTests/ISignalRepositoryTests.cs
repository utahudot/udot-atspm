using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using ATSPM.Infrastructure.Repositories;
using AutoFixture;
using AutoFixture.Xunit2;
using Castle.Core.Internal;
using Google.Api;
using Google.Apis.Util;
using InfrastructureTests.Attributes;
using InfrastructureTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using RTools_NTS.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace InfrastructureTests.RepositoryTests
{
    //[TestCaseOrderer("InfrastructureTests.Orderers.TraitValueTestCaseOrderer", "InfrastructureTests")]
    public class ISignalRepositoryTests : IClassFixture<EFContextFixture<ConfigContext>>, IDisposable
    {
        private const int SignalCount = 4;
        private const int ControllerTypeId = 1;
        
        private EFContextFixture<ConfigContext> _db;

        private readonly ITestOutputHelper _output;
        private ILogger<SignalEFRepository> _log;
        //private ConfigContext _db;
        private ISignalRepository _repo;

        private List<Signal> _signalList = new List<Signal>();

        public ISignalRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output)
        {
            _db = dbFixture;
            _output = output;
            
            _log = new Microsoft.Extensions.Logging.Abstractions.NullLogger<SignalEFRepository>();
            _repo = new SignalEFRepository(_db.Context, _log);

            SeedTestData();
        }

        private async void SeedTestData()
        {
            var fixture = new Fixture();

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            fixture.Customize<Signal>(c => c
                .Without(w => w.Id)
                .With(w => w.RegionId, 0)
                .Without(w => w.Region)
                .With(w => w.JurisdictionId, 0)
                .Without(w => w.Jurisdiction)
                .Without(w => w.ControllerTypeId)
                .Without(w => w.ControllerType)
                .Without(w => w.VersionAction)
                .Without(w => w.Approaches)
                .Without(w => w.Areas)
                .Without(w => w.MetricComments)
            );

            for (int x = 1; x <= SignalCount; x++)
            {
                string signalId = x.ToString();

                for (int i = 0; i <= Enum.GetValues(typeof(SignaVersionActions)).Length - 2; i++)
                {
                    var s = fixture.Create<Signal>();
                    s.SignalId = signalId;
                    s.VersionActionId = (SignaVersionActions)i;
                    s.PrimaryName = s.VersionActionId.ToString();
                    s.VersionAction = _db.Context.Set<VersionAction>().Find(s.VersionActionId);
                    s.ControllerTypeId = (i % 2 == 0) ? 1 : 2;
                    s.Start = DateTime.Today.AddDays((i - (i * 2)) - 1);
                    await _repo.AddAsync(s);

                    _output.WriteLine($"initial: {s.Id} - {s.SignalId} - {s.PrimaryName} - {s.VersionActionId} - {s.Start} - {s.ControllerTypeId}");
                }
            }

            _signalList = _repo.GetList().ToList();
        }

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

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public void ISignalRepositoryGetAllVersionsOfSignal(string signalId)
        {
            //string signalId = "1001";
            
            //var fixture = new Fixture();

            //fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
            //fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            //fixture.Customize<Signal>(c =>
            //    c.With(w => w.SignalId, signalId)
            //    .With(w => w.ControllerTypeId, 1)
            //    .Without(w => w.ControllerType)
            //    //.Without(w => w.Jurisdiction)
            //    //.Without(w => w.Region)
            //    .Without(w => w.VersionAction)
            //    .Without(w => w.Approaches)
            //    .Without(w => w.Areas)
            //    .Without(w => w.MetricComments)
            //);

            //var signalList = new List<Signal>();

            //for (int i = 0; i <= Enum.GetValues(typeof(SignaVersionActions)).Length - 2; i ++)
            //{
            //    var s = fixture.Create<Signal>();
            //    s.VersionActionId = (SignaVersionActions)i;
            //    s.PrimaryName = s.VersionActionId.ToString();
            //    s.VersionAction = _db.Context.Set<VersionAction>().Find(s.VersionActionId);
            //    signalList.Add(s);

            //    _output.WriteLine($"initial: {s.Id} - {s.SignalId} - {s.PrimaryName} - {s.VersionActionId} - {s.Start}");
            //}

            //await _repo.AddRangeAsync(signalList);

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
            //string signalId = "1002";

            //var fixture = new Fixture();

            //fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
            //fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            //fixture.Customize<Signal>(c =>
            //    c.With(w => w.SignalId, signalId)
            //    .With(w => w.ControllerTypeId, 1)
            //    .Without(w => w.ControllerType)
            //    //.Without(w => w.Jurisdiction)
            //    //.Without(w => w.Region)
            //    .Without(w => w.VersionAction)
            //    .Without(w => w.Approaches)
            //    .Without(w => w.Areas)
            //    .Without(w => w.MetricComments)
            //);

            //var signalList = new List<Signal>();

            //for (int i = 0; i <= Enum.GetValues(typeof(SignaVersionActions)).Length - 2; i++)
            //{
            //    var s = fixture.Create<Signal>();
            //    s.VersionActionId = (SignaVersionActions)i;
            //    s.PrimaryName = s.VersionActionId.ToString();
            //    s.VersionAction = _db.Context.Set<VersionAction>().Find(s.VersionActionId);
            //    signalList.Add(s);

            //    _output.WriteLine($"initial: {s.Id} - {s.SignalId} - {s.PrimaryName} - {s.VersionActionId} - {s.Start}");
            //}

            //await _repo.AddRangeAsync(signalList);

            var result = _repo.GetLatestVersionOfSignal(signalId);

            _output.WriteLine($"result: {result.Id} - {result.SignalId} - {result.PrimaryName} - {result.VersionActionId} - {result.Start}");

            //should not return deleted signals
            Assert.True(result.VersionActionId != SignaVersionActions.Delete);

            //all values should be signalId
            Assert.True(result.SignalId == signalId);

            //value should be newest date
            Assert.Equal(_signalList.Where(w => w.VersionActionId != SignaVersionActions.Delete && w.SignalId == signalId).Select(s => s.Start).OrderByDescending(o => o).FirstOrDefault(), result.Start);
        }

        [Fact]
        public void ISignalRepositoryGetLatestVersionOfAllSignals()
        {
            //var signalCount = 4;

            //var fixture = new Fixture();

            //fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
            //fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            //fixture.Customize<Signal>(c => c
            //    //c.With(w => w.SignalId, signalId)
            //    .With(w => w.ControllerTypeId, 1)
            //    .Without(w => w.ControllerType)
            //    //.Without(w => w.Jurisdiction)
            //    //.Without(w => w.Region)
            //    .Without(w => w.VersionAction)
            //    .Without(w => w.Approaches)
            //    .Without(w => w.Areas)
            //    .Without(w => w.MetricComments)
            //);

            //var signalList = new List<Signal>();

            //for (int x = 1; x <= signalCount; x++) 
            //{
            //    string signalId = x.ToString();

            //    for (int i = 0; i <= Enum.GetValues(typeof(SignaVersionActions)).Length - 2; i++)
            //    {
            //        var s = fixture.Create<Signal>();
            //        s.SignalId = signalId;
            //        s.VersionActionId = (SignaVersionActions)i;
            //        s.PrimaryName = s.VersionActionId.ToString();
            //        s.VersionAction = _db.Context.Set<VersionAction>().Find(s.VersionActionId);
            //        signalList.Add(s);

            //        _output.WriteLine($"initial: {s.Id} - {s.SignalId} - {s.PrimaryName} - {s.VersionActionId} - {s.Start}");
            //    }
            //}

            //await _repo.AddRangeAsync(signalList);

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
            //var signalCount = 4;
            //var controllerTypeId = 1;

            //var fixture = new Fixture();

            //fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
            //fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            //fixture.Customize<Signal>(c => c
            //    //c.With(w => w.SignalId, signalId)
            //    //.With(w => w.ControllerTypeId, 1)
            //    .Without(w => w.ControllerType)
            //    //.Without(w => w.Jurisdiction)
            //    //.Without(w => w.Region)
            //    .Without(w => w.VersionAction)
            //    .Without(w => w.Approaches)
            //    .Without(w => w.Areas)
            //    .Without(w => w.MetricComments)
            //);

            //var signalList = new List<Signal>();

            //for (int x = 1; x <= signalCount; x++)
            //{
            //    string signalId = x.ToString();

            //    for (int i = 0; i <= Enum.GetValues(typeof(SignaVersionActions)).Length - 2; i++)
            //    {
            //        var s = fixture.Create<Signal>();
            //        s.SignalId = signalId;
            //        s.VersionActionId = (SignaVersionActions)i;
            //        s.PrimaryName = s.VersionActionId.ToString();
            //        s.VersionAction = _db.Context.Set<VersionAction>().Find(s.VersionActionId);
            //        s.ControllerTypeId = (i % 2 == 0) ? 1 : 2;
            //        signalList.Add(s);

            //        _output.WriteLine($"initial: {s.Id} - {s.SignalId} - {s.PrimaryName} - {s.VersionActionId} - {s.Start} - {s.ControllerTypeId}");
            //    }
            //}

            //await _repo.AddRangeAsync(signalList);

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

        public void Dispose()
        {
            //_output.WriteLine($"Disposing database: {_db.GetHashCode()}");
            //_db.Dispose();
        }
    }
}
