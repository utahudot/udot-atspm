#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.RepositoryTests/RepositoryTestBase.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.ConfigurationModels;
using ATSPM.Domain.Services;
using ATSPM.Infrastructure.Repositories;
using AutoFixture;
using Google.Api;
using InfrastructureTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace InfrastructureTests.RepositoryTests
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database#repository-pattern
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public abstract class RepositoryTestBase<T1, T2, T3, T4> : IClassFixture<EFContextFixture<T3>> 
        where T1 : AtspmConfigModelBase<T4>, new()
        where T3 : DbContext, new()
    {
        protected EFContextFixture<T3> _db;
        protected ITestOutputHelper _output;
        protected T2 _repo;

        public RepositoryTestBase(EFContextFixture<T3> dbFixture, ITestOutputHelper output)
        {
            _db = dbFixture;
            _output = output;

            var T = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("ATSPM.Infrastructure"))
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(T2).IsAssignableFrom(p))
                .Where(p => typeof(ATSPMRepositoryEFBase<T1>).IsAssignableFrom(p))
                .FirstOrDefault();

            _repo = (T2)Activator.CreateInstance(T, _db.Context, null);

            //_db.Context.Database.ExecuteSqlRaw($"DElETE FROM {_db.Context.Model.FindEntityType(typeof(T1)).GetTableName()}");

            SeedTestData();
        }

        public CustomFixture ModelFixture { get; private set; } = new CustomFixture();

        protected abstract void SeedTestData();

        //[Fact]
        //public void IAsyncRepositoryAdd()
        //{
        //    var expected = ModelFixture.Create<T1>();
        //    T1 actual = null;
            
        //    if (_repo is IAsyncRepository<T1> repo)
        //    {
        //        repo.Add(expected);

        //        actual = repo.Lookup(expected);

        //        Assert.Equal(expected, actual);
        //    }
        //    else
        //    {
        //        Assert.False(true);
        //    }
        //}

        //[Fact]
        //public void IAsyncRepositoryRemove()
        //{
        //    var expected = ModelFixture.Create<T1>();
        //    T1 actual = null;

        //    if (_repo is IAsyncRepository<T1> repo)
        //    {
        //        repo.Add(expected);

        //        actual = repo.Lookup(expected);

        //        Assert.Equal(expected, actual);

        //        repo.Remove(actual);

        //        actual = repo.Lookup(expected);

        //        Assert.NotEqual(expected, actual);
        //    }
        //    else
        //    {
        //        Assert.False(true);
        //    }
        //}
    }

    public class CustomFixture : Fixture
    {
        public CustomFixture() 
        {
            this.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => this.Behaviors.Remove(b));
            this.Behaviors.Add(new OmitOnRecursionBehavior());

            this.Customize<Approach>(c => c
            .Without(w => w.Id)
            .Without(w => w.LocationId)
            .Without(w => w.DirectionType)
            .Without(w => w.Location)
            .Without(w => w.Detectors)
            );

            this.Customize<Area>(c => c
            .Without(w => w.Id)
            .Without(w => w.Locations)
            );

            //this.Customize<ControllerLogArchive>(c => c
            //);

            //this.Customize<ControllerType>(c => c
            //.Without(w => w.Id)
            //.Without(w => w.Locations)
            //);

            this.Customize<DetectorComment>(c => c
            .Without(w => w.Id)
            .Without(w => w.DetectorId)
            .Without(w => w.Detector)
            );

            this.Customize<Detector>(c => c
            .Without(w => w.Id)
            .Without(w => w.ApproachId)
            .Without(w => w.Approach)
            .Without(w => w.DetectionHardware)
            .Without(w => w.LaneType)
            .Without(w => w.MovementType)
            .Without(w => w.DetectorComments)
            .Without(w => w.DetectionTypes)
            );

            this.Customize<Location>(c => c
                    .Without(w => w.Id)
                    .With(w => w.RegionId, 0)
                    .Without(w => w.Region)
                    .With(w => w.JurisdictionId, 0)
                    .Without(w => w.Jurisdiction)
                    //.Without(w => w.ControllerTypeId)
                    //.Without(w => w.ControllerType)
                    .Without(w => w.VersionAction)
                    .Without(w => w.Approaches)
                    .Without(w => w.Areas)
                    //.Without(w => w.MetricComments)
                );
        }
    }
}
