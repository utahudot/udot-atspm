﻿using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IRouteSignalsRepository : IAsyncRepository<RouteSignal>
    {
        RouteSignal GetByRouteSignalId(int id);
        
        void MoveRouteSignalUp(int routeId, int routeSignalId);
        
        void MoveRouteSignalDown(int routeId, int routeSignalId);

        [Obsolete("Use GetList instead")]
        IReadOnlyList<RouteSignal> GetAllRoutesDetails();

        [Obsolete("Use Lookup instead")]
        IReadOnlyList<RouteSignal> GetByRouteID(int routeID);
        
        [Obsolete("Use Delete in the BaseClass")]
        void DeleteByRouteID(int routeID);
        
        [Obsolete("Use Delete in the BaseClass")]
        void DeleteById(int id);
        
        [Obsolete("Use Update in the BaseClass")]
        void UpdateByRouteAndApproachID(int routeID, string signalId, int newOrderNumber);
        
        [Obsolete("Use Add in the BaseClass")]
        void Add(RouteSignal newRouteDetail);
    }
}