using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Domain.Extensions;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using System;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IHourlySpeedRepository"/>
    public class HourlySpeedBQRepository : ATSPMRepositoryBQBase<HourlySpeed>, IHourlySpeedRepository
    {
        /// <inheritdoc/>
        public HourlySpeedBQRepository(ConfigContext db, ILogger<HourlySpeedBQRepository> log) : base(db, log) { }

        #region Overrides
        protected override BigQueryInsertRow CreateRow(HourlySpeed item)
        {
            return new BigQueryInsertRow
            {
                { "Date", item.Date },
                {"BinStartTime", item.BinStartTime },
                {"RouteId", item.RouteId },
                {"SourceId", item.SourceId },
                {"ConfidenceId", item.ConfidenceId },
                {"Average", item.Average },
                {"FifteenthSpeed", item.FifteenthSpeed },
                {"EightyFifthSpeed", item.EightyFifthSpeed },
                {"NinetyFifthSpeed", item.NinetyFifthSpeed },
                {"NinetyNinthSpeed", item.NinetyNinthSpeed },
                {"Violation", item.Violation },
                {"Flow", item.Flow }
            };
        }

        protected override HourlySpeed MapRowToEntity(BigQueryRow row)
        {
            return new HourlySpeed
            {
                Date = DateOnly.FromDateTime(row.GetPropertyValue<DateTime>("Id")),
                BinStartTime = TimeOnly.FromDateTime(row.GetPropertyValue<DateTime>("BinStartTime")),
                RouteId = row.GetPropertyValue<int>("RouteId"),
                SourceId = row.GetPropertyValue<int>("SourceId"),
                ConfidenceId = row.GetPropertyValue<int>("ConfidenceId"),
                Average = row.GetPropertyValue<int>("Average"),
                FifteenthSpeed = row.GetPropertyValue<int?>("FifteenthSpeed"),
                EightyFifthSpeed = row.GetPropertyValue<int?>("EightyFifthSpeed"),
                NinetyFifthSpeed = row.GetPropertyValue<int?>("NinetyFifthSpeed"),
                NinetyNinthSpeed = row.GetPropertyValue<int?>("NinetyNinthSpeed"),
                Violation = row.GetPropertyValue<int?>("Violation"),
                Flow = row.GetPropertyValue<int?>("Flow")
            };
        }



        #endregion

        #region IApproachRepository

        #endregion
    }
}