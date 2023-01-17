﻿using ATSPM.Data.Models;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    public interface IPriorityAggregationDatasRepository:IAggregationRepositoryBase
    {
        List<PriorityAggregation> GetPriorityAggregationByVersionIdAndDateRange(int versionId, DateTime start,
            DateTime end);

        PriorityAggregation Add(PriorityAggregation priorityAggregation);
        void Update(PriorityAggregation priorityAggregation);
        void Remove(PriorityAggregation priorityAggregation);
        List<PriorityAggregation> GetPriorityBySignalIdAndDateRange(string signalId, DateTime start, DateTime end);
    }
}