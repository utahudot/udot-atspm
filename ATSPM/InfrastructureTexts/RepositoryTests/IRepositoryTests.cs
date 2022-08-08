using ATSPM.Data.Models
using ATSPM.Domain.Services;
using ATSPM.Infrasturcture.Data;
using ATSPM.Infrasturcture.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace InfrastructureTests.RepositoryTests
{
    public class IRepositoryTests : IDisposable
    {
        private readonly DbConnection _connection;

        //const string connectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

        private readonly ITestOutputHelper _output;
        private ILogger<SignalEFRepository> _nullLogger;
        private MOEContext _db;
        private IRepository<Signal> _repo;

        public IRepositoryTests(ITestOutputHelper output)
        {
            _output = output;
            _nullLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<SignalEFRepository>();

            _connection = new SqliteConnection("Datasource=:memory:");
            _connection.Open();
            _db = new MOEContext(new DbContextOptionsBuilder<MOEContext>().EnableSensitiveDataLogging().UseSqlite(_connection).Options);

            _repo = new SignalEFRepository(_db, _nullLogger);

            _db.Database.EnsureDeleted();

            _output.WriteLine($"Created database: {_db.Database.EnsureCreated()}");
        }

        [Fact]
        public void IRepositoryAdd()
        {
            var signal = new Signal()
            {
                SignalId = "1234",
                Latitude = " ",
                Longitude = " ",
                PrimaryName = "Test Signal",
                SecondaryName = " ",
                Ipaddress = " ",
                RegionId = 1,
                ControllerTypeID = 1,
                //Enabled = true,
                VersionId = 3,
                VersionActionId = 10,
                Note = "Initial",
                Start = default
            };

            _repo.Add(signal);

            var collection = _repo.GetList(i => i.SignalId == signal.SignalId);

            Assert.Collection(collection,
                new Action<Signal>[]
                {
                    i =>
                    {
                        Assert.Equal(expected: signal.PrimaryName, actual: i.PrimaryName);
                    }
                });
        }

        [Fact]
        public void IRepositoryAddRange()
        {
            List<Signal> signals = new List<Signal>();
            
            for (int i = 1; i <= 5; i++)
            {
                var signal = new Signal()
                {
                    SignalId = i.ToString(),
                    Latitude = " ",
                    Longitude = " ",
                    PrimaryName = $"name:{i}",
                    SecondaryName = " ",
                    Ipaddress = " ",
                    RegionId = 1,
                    ControllerTypeID = 1,
                    //Enabled = true,
                    VersionId = i,
                    VersionActionId = 10,
                    Note = "Initial",
                    Start = default
                };

                signals.Add(signal);
            }

            _repo.AddRange(signals);

            var collection = _repo.GetList(i => true);

            Assert.Collection(collection,
                i => Assert.Equal(expected: "name:1", actual: i.PrimaryName),
                i => Assert.Equal(expected: "name:2", actual: i.PrimaryName),
                i => Assert.Equal(expected: "name:3", actual: i.PrimaryName),
                i => Assert.Equal(expected: "name:4", actual: i.PrimaryName),
                i => Assert.Equal(expected: "name:5", actual: i.PrimaryName));
        }

        [Fact]
        public void IRepositoryRemove()
        {
            var signal = new Signal()
            {
                SignalId = "1234",
                Latitude = " ",
                Longitude = " ",
                PrimaryName = "Test Signal",
                SecondaryName = " ",
                Ipaddress = " ",
                RegionId = 1,
                ControllerTypeID = 1,
                //Enabled = true,
                VersionId = 3,
                VersionActionId = 10,
                Note = "Initial",
                Start = default
            };

            _repo.Add(signal);

            var actual = _repo.GetList(i => i.SignalId == signal.SignalId).First();

            Assert.Equal(expected: signal.SignalId, actual: actual.SignalId);

            _repo.Remove(signal);

            Assert.True(_repo.GetList(i => i.SignalId == signal.SignalId).Count() == 0);
        }

        [Fact]
        public void IRepositoryRemoveRange()
        {
            List<Signal> signals = new List<Signal>();

            for (int i = 1; i <= 5; i++)
            {
                var signal = new Signal()
                {
                    SignalId = i.ToString(),
                    Latitude = " ",
                    Longitude = " ",
                    PrimaryName = $"name:{i}",
                    SecondaryName = " ",
                    Ipaddress = " ",
                    RegionId = 1,
                    ControllerTypeID = 1,
                    //Enabled = true,
                    VersionId = i,
                    VersionActionId = 10,
                    Note = "Initial",
                    Start = default
                };

                signals.Add(signal);
            }

            _repo.AddRange(signals);

            var collection = _repo.GetList(i => true);

            Assert.Collection(collection,
                i => Assert.Equal(expected: "name:1", actual: i.PrimaryName),
                i => Assert.Equal(expected: "name:2", actual: i.PrimaryName),
                i => Assert.Equal(expected: "name:3", actual: i.PrimaryName),
                i => Assert.Equal(expected: "name:4", actual: i.PrimaryName),
                i => Assert.Equal(expected: "name:5", actual: i.PrimaryName));

            _repo.RemoveRange(collection);

            Assert.True(_repo.GetList(i => true).Count() == 0);
        }

        [Fact]
        public void IRepositoryGetListFromExpression()
        {
            List<Signal> signals = new List<Signal>();

            for (int i = 1; i <= 5; i++)
            {
                var signal = new Signal()
                {
                    SignalId = i.ToString(),
                    Latitude = " ",
                    Longitude = " ",
                    PrimaryName = $"name:{i}",
                    SecondaryName = " ",
                    Ipaddress = " ",
                    RegionId = 1,
                    ControllerTypeID = 1,
                    //Enabled = true,
                    VersionId = i,
                    VersionActionId = 10,
                    Note = "Initial",
                    Start = default
                };

                signals.Add(signal);
            }

            _repo.AddRange(signals);

            Assert.True(_repo.GetList(i => true).Count() > 0);
        }

        [Fact(Skip = "no specifications to test")]
        public void IRepositoryGetListFromSpecification()
        {
            Assert.False(false);
        }

        [Fact]
        public void IRepositoryLookup()
        {
            var signal = new Signal()
            {
                SignalId = "1234",
                Latitude = " ",
                Longitude = " ",
                PrimaryName = "Test Signal",
                SecondaryName = " ",
                Ipaddress = " ",
                RegionId = 1,
                ControllerTypeID = 1,
                //Enabled = true,
                VersionId = 3,
                VersionActionId = 10,
                Note = "Initial",
                Start = default
            };

            _repo.Add(signal);

            var result = _repo.Lookup(signal);

            Assert.Equal(signal.SignalId, result.SignalId);
        }

        public void Dispose()
        {
            if (_db is IDisposable d)
            {
                d.Dispose();
            }

            _output.WriteLine($"Disposing database: {_db.GetHashCode()}");

            _repo = null;
            _db = null;
            _nullLogger = null;
        }
    }
}
