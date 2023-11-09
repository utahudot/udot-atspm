using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    public interface IAggregationRepositoryBase 
    {
        DateTime? GetLastAggregationDate();
    }
}