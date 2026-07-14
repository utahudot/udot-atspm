using Utah.Udot.Atspm.Data.Models.ConfigurationModels;

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Map Layer
    /// </summary>
    public partial class MapLayer : AtspmConfigModelBase<int>
    {
        /// <summary>
        /// Map name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Service Type
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// Show By Default
        /// </summary>
        public bool ShowByDefault { get; set; }

        /// <summary>
        /// Base endpoint URL.
        /// </summary>
        public string? MapLayerUrl { get; set; } = null;

        /// <summary>
        /// Resource identifier for the service.
        /// </summary>
        public string? ResourceId { get; set; }

        /// <summary>
        /// Optional style hint.
        /// </summary>
        public string? Style { get; set; }

        /// <summary>
        /// The interval at which the map layer should refresh, in seconds.
        /// </summary>
        public int? RefreshIntervalSeconds { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"{Id} - {Name}";
    }
}
