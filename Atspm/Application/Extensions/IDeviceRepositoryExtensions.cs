using Utah.Udot.Atspm.Configuration;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IDeviceRepository"/>
    /// </summary>
    public static class IDeviceRepositoryExtensions
    {
        /// <summary>
        /// <see cref="Device"/> query builder for the EventLogUtility.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="queryOptions"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<Device> GetDevicesForLogging(this IDeviceRepository repo, DeviceEventLoggingQueryOptions queryOptions)
        {
            var result = repo.GetList()
                .Where(w => w.LoggingEnabled)
                .Where(w => queryOptions.DeviceType == DeviceTypes.Unknown ? true : w.DeviceType == queryOptions.DeviceType)
                .Where(w => queryOptions.DeviceStatus == DeviceStatus.Unknown ? true : w.DeviceStatus == queryOptions.DeviceStatus)
                .Where(w => queryOptions.TransportProtocol == TransportProtocols.Unknown ? true : w.DeviceConfiguration.Protocol == queryOptions.TransportProtocol)
                .ToList()
                .Where(w => (queryOptions.IncludedLocations?.Count() > 0) ? queryOptions.IncludedLocations.Any(a => w.Location.LocationIdentifier == a) : true)
                .Where(w => (queryOptions.ExcludedLocations?.Count() > 0) ? !queryOptions.ExcludedLocations.Any(a => w.Location.LocationIdentifier == a) : true)
                .Where(w => (queryOptions.IncludedAreas?.Count() > 0) ? queryOptions.IncludedAreas.Any(l => w.Location.Areas.Any(a => a.Name == l)) : true)
                .Where(w => (queryOptions.IncludedRegions?.Count() > 0) ? queryOptions.IncludedRegions.Any(a => w.Location.Region.Description == a) : true)
                .Where(w => (queryOptions.IncludedJurisdictions?.Count() > 0) ? queryOptions.IncludedJurisdictions.Any(a => w.Location.Jurisdiction.Name == a) : true)
                .Where(w => (queryOptions.IncludedLocationTypes?.Count() > 0) ? queryOptions.IncludedLocationTypes.Any(a => w.Location.LocationType.Name == a) : true);

            return result.ToAsyncEnumerable();
        }
    }
}
