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
    public class IControllerTypeRepositoryTests : RepositoryTestBase<ControllerType, IControllerTypeRepository, ConfigContext>
    {
        private List<ControllerType> _list = new List<ControllerType>();

        public IControllerTypeRepositoryTests(EFContextFixture<ConfigContext> dbFixture, ITestOutputHelper output) : base(dbFixture, output) { }

        protected override void SeedTestData()
        {
            
        }

        #region IControllerTypeRepository

        #endregion

        #region IControllerTypeRepositoryExtensions

        #endregion
    }
}
