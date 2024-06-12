#region license
// Copyright 2024 Utah Departement of Transportation
// for InfrastructureTests - InfrastructureTests.RepositoryTests/IApproachRepositoryTests.cs
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
    public class IApproachRepositoryTests : RepositoryTestBase<Approach, IApproachRepository, ConfigContext, int>
    {
        private List<Approach> _list = new List<Approach>();

        public IApproachRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output) : base(dbFixture, output) { }

        protected override async void SeedTestData()
        {
            if (_repo.GetList().Count() < 1)
            {
                for (int x = 1; x <= Enum.GetValues(typeof(DirectionTypes)).Length - 1; x++)
                {
                    var s = ModelFixture.Create<Location>();
                    s.Id = x + 1000;
                    
                    var f = ModelFixture.Create<Approach>();
                    f.LocationId = s.Id;
                    f.Location = s;

                    await _repo.AddAsync(f);
                }
            }

            _list = _repo.GetList().ToList();

            foreach (var s in _list)
            {
                _output.WriteLine($"Seed Data: {s.Id} - {s.LocationId} - {s.Description} - {s.DirectionTypeId}");
            }
        }

        #region IApproachRepository

        #endregion

        #region IApproachRepositoryExtensions

        [Fact]
        public void IApproachRepositoryGetApproachesByIds()
        {
            var result = _repo.GetApproachesByIds(_list.Select(i => i.Id));

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Id} - {r.LocationId} - {r.Description} - {r.DirectionTypeId}");
            }

            //compare to initial collection
            Assert.Equal(_list, result);
        }

        #endregion
    }
}
