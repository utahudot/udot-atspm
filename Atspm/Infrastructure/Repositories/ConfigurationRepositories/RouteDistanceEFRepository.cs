#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories/RouteDistanceEFRepository.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IRouteDistanceRepository"/>
    public class RouteDistanceEFRepository : ATSPMRepositoryEFBase<RouteDistance>, IRouteDistanceRepository
    {
        /// <inheritdoc/>
        public RouteDistanceEFRepository(ConfigContext db, ILogger<RouteDistanceEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<RouteDistance> GetList()
        {
            return base.GetList()
                .Include(i => i.PreviousLocations)
                .Include(i => i.NextLocations);
        }

        #endregion

        #region IRouteDistanceRepository

        /// <inheritdoc/>
        public RouteDistance GetRouteDistanceByLocationIdentifiers(string locationA, string locationB)
        {
            return GetList().FirstOrDefault(w => w.LocationIdentifierA == locationA &&
            w.LocationIdentifierB == locationB ||
            w.LocationIdentifierA == locationB &&
            w.LocationIdentifierB == locationA);
        }

        #endregion
    }
}
