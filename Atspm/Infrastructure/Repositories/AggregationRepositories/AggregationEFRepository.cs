#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Repositories.AggregationRepositories/AggregationEFRepository.cs
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
using Utah.Udot.Atspm.Data;
    
namespace Utah.Udot.Atspm.Infrastructure.Repositories.AggregationRepositories
{
    /// <inheritdoc cref="ICompressedDataRepository{T}"/>
    public abstract class CompressedDataEFRepositoryBase<T>(DbContext db, ILogger<ATSPMRepositoryEFBase<T>> log) : ATSPMRepositoryEFBase<T>(db, log), ICompressedDataRepository<T> where T : CompressedDataBase
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
                .FromSpecification(new CompressedDataSpecification<T>(locationIdentifier, start, end))
                .Where(w => w.DataType == dataType)
                .AsAsyncEnumerable();
        }

        #endregion
    }

    ///<inheritdoc cref="IAggregationRepository"/>
    public class AggregationEFRepository(AggregationContext db, ILogger<AggregationEFRepository> log) : CompressedDataEFRepositoryBase<CompressedAggregationBase>(db, log), IAggregationRepository
    {
        #region IAggregationRepository

        /////<inheritdoc/>
        //public IAsyncEnumerable<CompressedAggregationBase> GetData(string locationIdentifier, DateTime start, DateTime end)
        //{
        //    return GetList()
        //        .FromSpecification(new CompressedAggregationsSpecification(locationIdentifier, start, end))
        //        .AsAsyncEnumerable();
        //}

        /////<inheritdoc/>
        //public IAsyncEnumerable<CompressedAggregationBase> GetData(string locationIdentifier, DateTime start, DateTime end, Type dataType)
        //{
        //    return GetList()
        //        .FromSpecification(new CompressedAggregationsSpecification(locationIdentifier, start, end))
        //        .Where(w => w.DataType == dataType)
        //        .AsAsyncEnumerable();
        //}

        ///<inheritdoc/>
        public IAsyncEnumerable<CompressedAggregations<T>> GetData<T>(string locationIdentifier, DateTime start, DateTime end) where T : AggregationModelBase
        {
            var type = typeof(T);

            return GetList()
                .FromSpecification(new CompressedDataSpecification<CompressedAggregationBase>(locationIdentifier, start, end))
                .Where(w => w.DataType == type)
                .Cast<CompressedAggregations<T>>()
                .AsAsyncEnumerable();
        }

        #endregion
    }
}
