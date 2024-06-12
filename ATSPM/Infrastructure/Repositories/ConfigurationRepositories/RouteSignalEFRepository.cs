#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Repositories.ConfigurationRepositories/RouteSignalEFRepository.cs
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
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IRouteLocationsRepository"/>
    public class RouteLocationEFRepository : ATSPMRepositoryEFBase<RouteLocation>, IRouteLocationsRepository
    {
        /// <inheritdoc/>
        public RouteLocationEFRepository(ConfigContext db, ILogger<RouteLocationEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<RouteLocation> GetList()
        {
            return base.GetList()
                .Include(i => i.Route)
                .Include(i => i.PrimaryDirection)
                .Include(i => i.OpposingDirection)
                .Include(i => i.PreviousLocationDistance)
                .Include(i => i.NextLocationDistance)
                .OrderBy(o => o.Order);
        }

        #endregion

        #region IRouteLocationsRepository

        public RouteLocation GetByRoutelocationId(int id)
        {
            throw new System.NotImplementedException();
        }

        public void MoveRouteLocationDown(int routeId, int routelocationId)
        {
            throw new System.NotImplementedException();
        }

        public void MoveRouteLocationUp(int routeId, int routelocationId)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
