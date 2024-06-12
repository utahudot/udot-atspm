#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.RepositoryTests/IAreaRepositoryTests.cs
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
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.ConfigurationRepositories;
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
    public class IAreaRepositoryTests : RepositoryTestBase<Area, IAreaRepository, ConfigContext, int>
    {
        private const int ItemCount = 4;

        private List<Area> _list = new List<Area>();

        public IAreaRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output) : base(dbFixture, output) { }

        protected override async void SeedTestData()
        {
            if (_repo.GetList().Count() < ItemCount)
            {
                for (int x = 1; x <= ItemCount; x++)
                {
                    var f = ModelFixture.Create<Area>();
                    f.Id = x;
                    f.Name = $"Area-{x}";

                    await _repo.AddAsync(f);
                }
            }

            _list = _repo.GetList().ToList();

            foreach (var s in _list)
            {
                _output.WriteLine($"Seed Data: {s.Id} - {s.Name}");
            }
        }

        #region IAreaRepository

        #endregion

        #region IAreaRepositoryExtensions

        [Theory]
        [InlineData("Area-1")]
        [InlineData("Area-2")]
        [InlineData("Area-3")]
        [InlineData("Area-4")]
        public void IAreaRepositoryGetAreaByName(string areaName)
        {
            var result = _repo.GetAreaByName(areaName);

            _output.WriteLine($"result: {result.Id} - {result.Name}");

            Assert.Equal(areaName, result.Name);
        }

        #endregion
    }
}