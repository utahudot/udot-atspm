#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Repositories.AggregationRepositories/CompressedDataEFRepositoryBase.cs
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.AggregationRepositories
{
    /// <inheritdoc cref="ICompressedDataRepository{T}"/>
    public abstract class CompressedDataEFRepositoryBase<T>(DbContext db, ILogger<CompressedDataEFRepositoryBase<T>> log) : ATSPMRepositoryEFBase<T>(db, log), ICompressedDataRepository<T> where T : CompressedDataBase
    {
        #region ICompressedDataRepository

        /// <inheritdoc cref="ICompressedDataRepository{T}.GetData(string, DateTime, DateTime)"/>
        public IAsyncEnumerable<T> GetData(string locationIdentifier, DateTime start, DateTime end)
        {
            return GetList()
                .FromSpecification(new CompressedDataSpecification<T>(locationIdentifier, start, end))
                .AsAsyncEnumerable();
        }

        /// <inheritdoc cref="ICompressedDataRepository{T}.GetData(string, DateTime, DateTime, Type)"/>
        public IAsyncEnumerable<T> GetData(string locationIdentifier, DateTime start, DateTime end, Type dataType)
        {
            return GetList()
                .FromSpecification(new CompressedDataSpecification<T>(locationIdentifier, start, end, dataType))
                .AsAsyncEnumerable();
        }

        #endregion
    }
}
