#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Repositories.AggregationRepositories/ApproachYellowRedActivationAggregationEFRepository.cs
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
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IApproachYellowRedActivationAggregationRepository"/>
    public class ApproachYellowRedActivationAggregationEFRepository : AggregationEFRepositoryBase<ApproachYellowRedActivationAggregation>, IApproachYellowRedActivationAggregationRepository
    {
        ///<inheritdoc/>
        public ApproachYellowRedActivationAggregationEFRepository(AggregationContext db, ILogger<ApproachYellowRedActivationAggregationEFRepository> log) : base(db, log) { }

        #region IApproachYellowRedActivationAggregationRepository

        #endregion
    }
}
