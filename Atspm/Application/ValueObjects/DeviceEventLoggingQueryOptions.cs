using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.ValueObjects
{
    /// <summary>
    /// Options for querying which <see cref="Device"/> to query.
    /// </summary>
    public class DeviceEventLoggingQueryOptions
    {
        /// <summary>
        /// List of <see cref="Location.LocationIdentifier"/> to include
        /// </summary>
        public IEnumerable<string> IncludedLocations { get; set; }

        /// <summary>
        /// List of <see cref="Location.LocationIdentifier"/> to exclude
        /// </summary>
        public IEnumerable<string> ExcludedLocations { get; set; }

        /// <summary>
        /// List of <see cref="Area.Name"/> to include
        /// </summary>
        public IEnumerable<string> IncludedAreas { get; set; }

        /// <summary>
        /// List of <see cref="Jurisdiction.Name"/> to include
        /// </summary>
        public IEnumerable<string> IncludedJurisdictions { get; set; }

        /// <summary>
        /// List of <see cref="Region.Description"/> to include
        /// </summary>
        public IEnumerable<string> IncludedRegions { get; set; }

        /// <summary>
        /// List of <see cref="LocationType.Name"/> to include
        /// </summary>
        public IEnumerable<string> IncludedLocationTypes { get; set; }

        /// <summary>
        /// <see cref="DeviceTypes"/> to include
        /// </summary>
        public DeviceTypes DeviceType { get; set; } = DeviceTypes.Unknown;

        /// <summary>
        /// <see cref="TransportProtocols"/> to include
        /// </summary>
        public TransportProtocols TransportProtocol { get; set; }

        /// <summary>
        /// <see cref="DeviceStatus"/> to include
        /// </summary>
        public DeviceStatus DeviceStatus { get; set; } = DeviceStatus.Active;
    }
}
