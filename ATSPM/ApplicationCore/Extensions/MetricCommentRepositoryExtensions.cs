using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class MetricCommentRepositoryExtensions
    {
        public static MetricComment GetLatestCommentForReport(this IMetricCommentRepository repo, string signalId, int metricId)
        {
            return repo.GetList()
                .Where(w => w.SignalIdentifier == signalId && (w.MetricTypes.Where(i => i.Id == metricId).Count() > 0))
                .OrderByDescending(o => o.TimeStamp)
                .FirstOrDefault();
        }
    }
}
