using ATSPM.Data.Models
using ATSPM.Domain.Services;
using ATSPM.Infrastructure.Data;
using ATSPM.Infrastructure.Repositories;
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
        private ILogger<LocationEFRepository> _nullLogger;
        private MOEContext _db;
        private IRepository<Location> _repo;

        public IRepositoryTests(ITestOutputHelper output)
        {
            _output = output;
            _nullLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<LocationEFRepository>();

            _connection = new SqliteConnection("Datasource=:memory:");
            _connection.Open();
            _db = new MOEContext(new DbContextOptionsBuilder<MOEContext>().EnableSensitiveDataLogging().UseSqlite(_connection).Options);

            _repo = new LocationEFRepository(_db, _nullLogger);

            _db.Database.EnsureDeleted();

            _output.WriteLine($"Created database: {_db.Database.EnsureCreated()}");
        }

        [Fact]
        public void IRepositoryAdd()
        {
            var Location = new Location()
            {
                locationId = "1234",
                Latitude = " ",
                Longitude = " ",
                PrimaryName = "Test Location",
                SecondaryName = " ",
                Ipaddress = " ",
                RegionId = 1,
                ControllerTypeId = 1,
                //Enabled = true,
                VersionId = 3,
                VersionActionId = 10,
                Note = "Initial",
                Start = default
            };

            _repo.Add(Location);

            var collection = _repo.GetList(i => i.locationId == Location.locationId);

            Assert.Collection(collection,
                new Action<Location>[]
                {
                    i =>
                    {
                        Assert.Equal(expected: Location.PrimaryName, actual: i.PrimaryName);
                    }
                });
        }

        [Fact]
        public void IRepositoryAddRange()
        {
            List<Location> Locations = new List<Location>();
            
            for (int i = 1; i <= 5; i++)
            {
                var Location = new Location()
                {
                    locationId = i.ToString(),
                    Latitude = " ",
                    Longitude = " ",
                    PrimaryName = $"name:{i}",
                    SecondaryName = " ",
                    Ipaddress = " ",
                    RegionId = 1,
                    ControllerTypeId = 1,
                    //Enabled = true,
                    VersionId = i,
                    VersionActionId = 10,
                    Note = "Initial",
                    Start = default
                };

                Locations.Add(Location);
            }

            _repo.AddRange(Locations);

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
            var Location = new Location()
            {
                locationId = "1234",
                Latitude = " ",
                Longitude = " ",
                PrimaryName = "Test Location",
                SecondaryName = " ",
                Ipaddress = " ",
                RegionId = 1,
                ControllerTypeId = 1,
                //Enabled = true,
                VersionId = 3,
                VersionActionId = 10,
                Note = "Initial",
                Start = default
            };

            _repo.Add(Location);

            var actual = _repo.GetList(i => i.locationId == Location.locationId).First();

            Assert.Equal(expected: Location.locationId, actual: actual.locationId);

            _repo.Remove(Location);

            Assert.True(_repo.GetList(i => i.locationId == Location.locationId).Count() == 0);
        }

        [Fact]
        public void IRepositoryRemoveRange()
        {
            List<Location> Locations = new List<Location>();

            for (int i = 1; i <= 5; i++)
            {
                var Location = new Location()
                {
                    locationId = i.ToString(),
                    Latitude = " ",
                    Longitude = " ",
                    PrimaryName = $"name:{i}",
                    SecondaryName = " ",
                    Ipaddress = " ",
                    RegionId = 1,
                    ControllerTypeId = 1,
                    //Enabled = true,
                    VersionId = i,
                    VersionActionId = 10,
                    Note = "Initial",
                    Start = default
                };

                Locations.Add(Location);
            }

            _repo.AddRange(Locations);

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
            List<Location> Locations = new List<Location>();

            for (int i = 1; i <= 5; i++)
            {
                var Location = new Location()
                {
                    locationId = i.ToString(),
                    Latitude = " ",
                    Longitude = " ",
                    PrimaryName = $"name:{i}",
                    SecondaryName = " ",
                    Ipaddress = " ",
                    RegionId = 1,
                    ControllerTypeId = 1,
                    //Enabled = true,
                    VersionId = i,
                    VersionActionId = 10,
                    Note = "Initial",
                    Start = default
                };

                Locations.Add(Location);
            }

            _repo.AddRange(Locations);

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
            var Location = new Location()
            {
                locationId = "1234",
                Latitude = " ",
                Longitude = " ",
                PrimaryName = "Test Location",
                SecondaryName = " ",
                Ipaddress = " ",
                RegionId = 1,
                ControllerTypeId = 1,
                //Enabled = true,
                VersionId = 3,
                VersionActionId = 10,
                Note = "Initial",
                Start = default
            };

            _repo.Add(Location);

            var result = _repo.Lookup(Location);

            Assert.Equal(Location.locationId, result.locationId);
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
