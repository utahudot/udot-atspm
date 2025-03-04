#region license
// Copyright 2025 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.Atspm.InfrastructureTests.Fixtures/EFContextFixtureBase.cs
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

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;

namespace Utah.Udot.Atspm.InfrastructureTests.Fixtures
{
    public class EFContextFixture<T> : IDisposable where T : DbContext, new()
    {
        private DbConnection _connection;

        public EFContextFixture()
        {
            _connection = new SqliteConnection("Datasource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<T>().EnableSensitiveDataLogging().UseSqlite(_connection).Options;

            Context = (T)Activator.CreateInstance(typeof(T), options);

            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();
        }

        public T Context { get; private set; }

        public void Dispose()
        {
            _connection.Dispose();
            Context.Dispose();
        }
    }

    //public class RepositoryFixture<T1, T2> : EFContextFixtureBase<T1> where T1 : DbContext, new() where T2 : ATSPMModelBase
    //{
    //    private IAsyncRepository<T2> _repo;

    //    public RepositoryFixture()
    //    {
    //        Context.Database.ExecuteSql($"DELETE FROM {typeof(T2).Name}");

    //        Repository = new 
    //    }

    //    public IAsyncRepository<T2> Repository { get; private set; }
    //}
}
